using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Byrone.Xenia.Internal;
using Byrone.Xenia.Internal.Extensions;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia
{
	public sealed partial class Server : System.IDisposable
	{
		private static System.ReadOnlySpan<byte> BadRequest =>
			"HTTP/1.1 400 Bad Request\n"u8;

		private readonly Config config;
		private readonly Socket socket;
		private readonly ArrayPool<byte> bufferPool;

		/// <summary>
		/// Create a new server instance.
		/// </summary>
		/// <param name="config">The configuration to use.</param>
		public Server(Config config)
		{
			this.config = config;

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
				this.Accept(token);
			}
		}

		private void Accept(CancellationToken token)
		{
			System.Span<byte> logBuffer = stackalloc byte[64];

			// @todo Use cancellation token, somehow
			var client = this.socket.Accept();

			this.Log(LogLevel.Debug, logBuffer, $"[{IPv4.From(client.RemoteEndPoint)}] Accepted new client");

			var buffer = new RentedArray(this.bufferPool, this.config.ReceiveBufferSize);

			var received = client.Receive(buffer, SocketFlags.None, out var code);

			this.Log(LogLevel.Debug,
					 logBuffer,
					 $"[{IPv4.From(client.RemoteEndPoint)}] Received {received} bytes, result code: {(int)code}");

			if ((code != SocketError.Success) || (received == 0))
			{
				this.Log(LogLevel.Warning,
						 logBuffer,
						 $"[{IPv4.From(client.RemoteEndPoint)}] Bad request ({received} bytes)");

				client.Send(Server.BadRequest);

				client.Dispose();

				return;
			}

			var context = new ClientContext(client, buffer, received);

			ThreadPool.QueueUserWorkItem(this.Handle, context);
		}

		private void Handle(object? state)
		{
			if (state is not ClientContext context)
			{
				return;
			}

			var client = context.Client;

			// @todo Parse request

			client.Send("HTTP/1.1 200 OK\n"u8);

			this.Log(LogLevel.Debug,
					 stackalloc byte[64],
					 $"[{IPv4.From(client.RemoteEndPoint)}] Sent response, closing.");

			context.Dispose();
		}

		public void Dispose() =>
			this.socket.Dispose();

		private readonly struct ClientContext : System.IDisposable
		{
			public readonly Socket Client;
			public readonly RentedArray Buffer;
			public readonly int RequestLength;

			public System.ReadOnlySpan<byte> Request =>
				this.Buffer.Span.SliceUnsafe(0, this.RequestLength);

			public ClientContext(Socket client, RentedArray buffer, int requestLength)
			{
				this.Client = client;
				this.Buffer = buffer;
				this.RequestLength = requestLength;
			}

			public void Dispose()
			{
				this.Buffer.Dispose();
				this.Client.Dispose();
			}
		}
	}
}
