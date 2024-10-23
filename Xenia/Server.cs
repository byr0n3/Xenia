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
		private readonly RequestHandler requestHandler;

		private readonly ArrayPool<byte> bufferPool;
		private readonly RentedArray<byte> logBuffer;

		private bool disposed;

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

			this.logBuffer = new RentedArray<byte>(128);
			this.bufferPool = ArrayPool<byte>.Create(config.ReceiveBufferSize, 10);
		}

		/// <summary>
		/// Bind to the <see cref="Config.Address"/> configured in the <see cref="Config"/> amd start accepting clients.
		/// </summary>
		/// <remarks>This call <b>IS</b> blocking and will block the current thread until the server gets disposed.</remarks>
		public void Run()
		{
			const int interruptedCode = 4;

			this.Bind();

			while (!this.disposed)
			{
				try
				{
					var client = this.socket.Accept();

					ThreadPool.QueueUserWorkItem(this.HandleClient, client);
				}
				catch (SocketException ex)
				{
					// If the ErrorCode is 4, the server got manually stopped, no error occured.
					if (ex.ErrorCode != interruptedCode)
					{
						this.Log(ex);
					}
				}
			}
		}

		/// <summary>
		/// Bind to the configured IP and port and start listening.
		/// </summary>
		/// <remarks>This is put in a separate function so the stackalloc-ed buffer instantly gets released.</remarks>
		private void Bind()
		{
			this.socket.Bind((EndPoint)this.config.Address);
			this.socket.Listen(this.config.Backlog);

			this.Log(LogLevel.Info, this.logBuffer, $"Listening on: http://{this.config.Address}");
		}

		/// <summary>
		/// Receive the request data from the given socket.
		/// </summary>
		/// <param name="state"><see cref="Socket"/> instance.</param>
		/// <remarks>This function gets called using <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object?)"/>, which is why the <paramref name="state"/> parameter is an object.</remarks>
		private void HandleClient(object? state)
		{
			if (state is not Socket client)
			{
				return;
			}

			var ticks = System.DateTime.UtcNow.Ticks;

			this.Log(LogLevel.Debug, this.logBuffer, $"[{IPv4.From(client.RemoteEndPoint)}] Accepted");

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
						 this.logBuffer,
						 $"[{IPv4.From(client.RemoteEndPoint)}] Bad request (parse error)");

				client.Send(Server.BadRequest);
			}

			buffer.Dispose();

			var elapsed = System.TimeSpan.FromTicks(System.DateTime.UtcNow.Ticks - ticks);

			this.Log(LogLevel.Debug,
					 this.logBuffer,
					 $"[{IPv4.From(client.RemoteEndPoint)}] Response sent ({elapsed.TotalMilliseconds}ms)");

			client.Dispose();
		}

		[MustDisposeResource]
		private RentedArray<byte> Receive(Socket client, out int received)
		{
			var buffer = new RentedArray<byte>(this.bufferPool, this.config.ReceiveBufferSize);

			received = client.Receive(buffer, SocketFlags.None, out var code);

			// Client might send some delayed data
			while ((received < buffer.Size) && client.Poll(this.config.PollInterval, SelectMode.SelectRead))
			{
				received += client.Receive(buffer.Span.Slice(received), SocketFlags.None, out _);
			}

			this.Log(LogLevel.Debug, this.logBuffer, $"[{IPv4.From(client.RemoteEndPoint)}] Received {received}b");

			// ReSharper disable once InvertIf
			if ((code != SocketError.Success) || (received == 0))
			{
				this.Log(LogLevel.Warning, this.logBuffer, $"[{IPv4.From(client.RemoteEndPoint)}] Error: {(int)code}");

				buffer.Dispose();

				received = default;
				return default;
			}

			return buffer;
		}

		public void Dispose()
		{
			if (this.disposed)
			{
				return;
			}

			this.disposed = true;

			this.socket.Dispose();
			this.logBuffer.Dispose();
		}
	}
}
