using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia
{
	public sealed class Server : System.IDisposable
	{
		private const int bufferSize = 512;

		private readonly TcpListener listener;
		private readonly ServerOptions options;
		private readonly List<RequestHandler> handlers; // @todo No list
		private readonly CancellationToken cancelToken;

		private IServerLogger Logger =>
			this.options.Logger;

		public Server(ServerOptions options, CancellationToken token)
		{
			if (!IPAddress.TryParse(options.IpAddress, out var ip))
			{
				throw new System.ArgumentException("Invalid IP address", nameof(options));
			}

			this.options = options;
			this.cancelToken = token;
			this.handlers = new List<RequestHandler>();
			this.listener = new TcpListener(ip, options.Port);

			this.listener.Start();

			this.Logger.LogInfo($"Server started on http://{options.IpAddress}:{options.Port}");
		}

		public void RegisterHandler(RequestHandler handler) =>
			this.handlers.Add(handler);

		public bool UnregisterHandler(RequestHandler handler) =>
			this.handlers.Remove(handler);

		public void Listen()
		{
			while (!this.cancelToken.IsCancellationRequested)
			{
				this.HandleConnection();
			}
		}

		private void HandleConnection()
		{
			var client = this.listener.AcceptTcpClient();
			var stream = client.GetStream();

			if (!stream.CanRead)
			{
				this.Logger.LogWarning("Received an unreadable stream");

				return;
			}

			System.Span<byte> buffer = stackalloc byte[Server.bufferSize];

			var read = stream.Read(buffer);

			var bytes = buffer.Slice(0, read);

			using (var ranges = new RentedArray<System.Range>(15)) // @todo
			{
				var count = bytes.Split(ranges.Data, (byte)'\n');

				if (count == 0 || !ServerHelpers.TryGetRequest(bytes, ranges.AsSpan(0, count), out var request))
				{
					this.Logger.LogWarning("Unable to parse request");

					return;
				}

				using (var writer = new ResponseWriter(ref stream))
				{
					var result = this.TryHandleRequest(writer, in request);

					switch (result)
					{
						case HandleRequestResult.MethodNotAllowed:
							writer.WriteMethodNotAllowed(in request);
							break;

						case HandleRequestResult.NotAllowed:
							writer.WriteNotFound(in request);
							break;
					}
				}

				stream.Flush();

				request.Dispose();
			}

			client.Close();
		}

		private HandleRequestResult TryHandleRequest(ResponseWriter writer, in Request request)
		{
			foreach (var handler in this.handlers)
			{
				if (handler.Path != request.Path)
				{
					continue;
				}

				if (handler.Method != request.Method)
				{
					return HandleRequestResult.MethodNotAllowed;
				}

				handler.Handler(writer, in request);
				return HandleRequestResult.Success;
			}

			return HandleRequestResult.NotAllowed;
		}

		public void Dispose()
		{
			this.Logger.LogInfo("Closing server...");

			this.listener.Dispose();
		}

		private enum HandleRequestResult
		{
			NotAllowed = 0,
			MethodNotAllowed,
			Success,
		}
	}
}
