using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Byrone.Xenia.Internal;
using Byrone.Xenia.Utilities;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	public sealed partial class Server : System.IDisposable
	{
		public delegate IResponse RequestHandler(in Request request);

		private static System.ReadOnlySpan<byte> BadRequest =>
			"HTTP/1.1 400 Bad Request\n"u8;

		private readonly Config config;
		private readonly Socket socket;
		private readonly ArrayPool<byte> bufferPool;
		private readonly RequestHandler requestHandler;

		/// <summary>
		/// Create a new server instance.
		/// </summary>
		/// <param name="config">The configuration to use.</param>
		/// <param name="requestHandler">The callback function to call to handle a request and create a response.</param>
		public Server(Config config, RequestHandler requestHandler)
		{
			this.config = config;
			this.requestHandler = requestHandler;

			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
			{
				ReceiveBufferSize = config.ReceiveBufferSize,
			};

			this.bufferPool = ArrayPool<byte>.Create(config.ReceiveBufferSize, 10);
		}

		/// <summary>
		/// Bind to the <see cref="Config.Address"/> configured in the <see cref="Config"/> amd start accepting clients.
		/// </summary>
		/// <param name="token">Cancellation token.</param>
		/// <remarks>This call <b>IS</b> blocking and will block the current thread until the given <paramref name="token"/> is cancelled.</remarks>
		public void Run(CancellationToken token = default)
		{
			this.socket.Bind((EndPoint)this.config.Address);
			this.socket.Listen(this.config.Backlog);

			this.Log(LogLevel.Info, stackalloc byte[64], $"Listening on: http://{this.config.Address}");

			while (!token.IsCancellationRequested)
			{
				// @todo Cancel when CancellationToken gets cancelled
				var client = this.socket.Accept();

				ThreadPool.QueueUserWorkItem(this.HandleClientData, client);
			}
		}

		/// <summary>
		/// Receive the request data from the given socket.
		/// </summary>
		/// <param name="state"><see cref="Socket"/> instance.</param>
		/// <remarks>This function gets called using <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object?)"/>, which is why the <paramref name="state"/> parameter is an object.</remarks>
		private void HandleClientData(object? state)
		{
			if (state is not Socket client)
			{
				return;
			}

			var ticks = System.DateTime.UtcNow.Ticks;

			System.Span<byte> logBuffer = stackalloc byte[64];

			this.Log(LogLevel.Debug, logBuffer, $"[{IPv4.From(client.RemoteEndPoint)}] Connected");

			var buffer = this.Receive(client, out var received);

			if (received == 0)
			{
				// Buffer is already disposed

				client.Send(Server.BadRequest);

				client.Dispose();

				return;
			}

			if (RequestParser.TryParse(buffer.Span.Slice(0, received), out var request))
			{
				var response = this.requestHandler(in request);

				response.Send(client);

				// ReSharper disable once SuspiciousTypeConversion.Global
				if (response is System.IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
			else
			{
				this.Log(LogLevel.Warning,
						 logBuffer,
						 $"[{IPv4.From(client.RemoteEndPoint)}] Bad request (parse error)");

				client.Send(Server.BadRequest);
			}

			buffer.Dispose();

			var elapsed = System.TimeSpan.FromTicks(System.DateTime.UtcNow.Ticks - ticks);

			this.Log(LogLevel.Debug,
					 logBuffer,
					 $"[{IPv4.From(client.RemoteEndPoint)}] Response sent, disconnecting ({elapsed.TotalMilliseconds}ms)");

			client.Dispose();
		}

		[MustDisposeResource]
		private RentedArray<byte> Receive(Socket client, out int received)
		{
			var buffer = new RentedArray<byte>(this.bufferPool, this.config.ReceiveBufferSize);

			received = client.Receive(buffer, SocketFlags.None, out var code);

			while (client.Poll(this.config.PollInterval, SelectMode.SelectRead))
			{
				received += client.Receive(buffer.Span.Slice(received), SocketFlags.None, out _);
			}

			this.Log(LogLevel.Debug,
					 stackalloc byte[64],
					 $"[{IPv4.From(client.RemoteEndPoint)}] Received {received} bytes, result code: {(int)code}");

			if ((code != SocketError.Success) || (received == 0))
			{
				this.Log(LogLevel.Warning,
						 stackalloc byte[64],
						 $"[{IPv4.From(client.RemoteEndPoint)}] Bad request: {(int)code} ({received} bytes)");

				buffer.Dispose();

				received = default;
				return default;
			}

			return buffer;
		}

		public void Dispose() =>
			this.socket.Dispose();
	}
}
