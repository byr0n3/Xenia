using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia
{
	public sealed class Server : System.IDisposable
	{
		private const int bufferSize = 1024;

		private readonly TcpListener listener;
		private readonly ServerOptions options;
		private readonly List<RequestHandler> handlers; // @todo No list
		private readonly CancellationToken cancelToken;
		private readonly CancellationTokenRegistration cancelRegistration;

		private IServerLogger? Logger =>
			this.options.Logger;

		public Server(ServerOptions options, CancellationToken token = default)
		{
			if (!IPAddress.TryParse(options.IpAddress, out var ip))
			{
				throw new System.ArgumentException("Invalid IP address", nameof(options));
			}

			this.options = options;
			this.cancelToken = token;
			this.handlers = new List<RequestHandler>();
			this.listener = new TcpListener(ip, options.Port);

			this.cancelRegistration = this.cancelToken.Register(this.Dispose);

			this.listener.Start();

			this.Logger?.LogInfo($"Server started on http://{options.IpAddress}:{options.Port}");
		}

		public void AddRequestHandler(in RequestHandler handler)
		{
			if (this.handlers.Contains(handler))
			{
				throw new System.ArgumentException("Request Handler with the same method and path already exists.",
												   nameof(handler));
			}

			this.handlers.Add(handler);
		}

		public bool RemoveRequestHandler(in RequestHandler handler) =>
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
			try
			{
				var client = this.listener.AcceptTcpClient();
				var stream = client.GetStream();

				if (!stream.CanRead)
				{
					this.Logger?.LogWarning("Received an unreadable stream");

					return;
				}

				// @todo Resizable
				var buffer = new RentedArray<byte>(Server.bufferSize);

				var read = stream.Read(buffer.AsSpan());

				var bytes = buffer.AsSpan().Slice(0, read);

				// @todo Resizable
				var ranges = new RentedArray<System.Range>(15);

				var count = bytes.Split(ranges.Data, (byte)'\n');

				if (count == 0 || !ServerHelpers.TryGetRequest(bytes, ranges.AsSpan(0, count), out var request))
				{
					this.Logger?.LogWarning("Unable to parse request");

					return;
				}

				var response = new ResponseBuilder();

				var handler = this.TryHandleRequest(in request);

				handler.Invoke(in request, ref response);

				// @todo Support compression/encoding (gZip, etc)
				stream.Write(response.Span);

				response.Dispose();

				request.Dispose();

				ranges.Dispose();

				client.Dispose();
			}
			catch (SocketException) when (this.cancelToken.IsCancellationRequested)
			{
				//
			}
		}

		private RequestHandler.RequestHandlerCallback TryHandleRequest(in Request request)
		{
			foreach (var handler in this.handlers)
			{
				if (handler.Path != request.Path)
				{
					continue;
				}

				if (handler.Method != request.Method)
				{
					return Server.MethodNotAllowedHandler;
				}

				return handler.Handler;
			}

			return Server.NotFoundHandler;
		}

		private static void MethodNotAllowedHandler(in Request request, ref ResponseBuilder response) =>
			response.AppendHtml(in request,
								in StatusCodes.Status405MethodNotAllowed,
								// @todo Customizable
								"<html><body><h1>405 Method Not Allowed</h1></html></body>"u8);

		private static void NotFoundHandler(in Request request, ref ResponseBuilder response) =>
			response.AppendHtml(in request,
								in StatusCodes.Status404NotFound,
								// @todo Customizable
								"<html><body><h1>404 Not Found</h1></html></body>"u8);

		public void Dispose()
		{
			this.Logger?.LogInfo("Closing server...");

			this.cancelRegistration.Dispose();

			this.listener.Dispose();
		}
	}
}
