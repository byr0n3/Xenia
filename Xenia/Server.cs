using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;
using Byrone.Xenia.Internal;

namespace Byrone.Xenia
{
	public sealed class Server : System.IDisposable
	{
		private const int bufferSize = 1024;

		internal readonly ServerOptions Options;

		private readonly TcpListener listener;
		private readonly List<RequestHandler> handlers; // @todo No list
		private readonly CancellationToken cancelToken;
		private readonly CancellationTokenRegistration cancelRegistration;

		private IServerLogger? Logger =>
			this.Options.Logger;

		public Server(ServerOptions options, CancellationToken token = default)
		{
			if (!IPAddress.TryParse(options.IpAddress, out var ip))
			{
				throw new System.ArgumentException("Invalid IP address", nameof(options));
			}

			this.Options = options;
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

				Debug.Assert(stream.CanRead);
				Debug.Assert(stream.CanWrite);

				// @todo ResizableRentedArray
				var buffer = new RentedArray<byte>(Server.bufferSize);

				var read = stream.Read(buffer.AsSpan());

				var bytes = buffer.AsSpan().Slice(0, read);

				// @todo ResizableRentedArray
				var ranges = new RentedArray<System.Range>(32);

				var count = bytes.Split(ranges.Data, Characters.NewLine);

				Debug.Assert(count != 0);

				var response = new ResponseBuilder();

				if (ServerHelpers.TryGetRequest(this, bytes, ranges.AsSpan(0, count), out var request))
				{
					request.HandlerCallback.Invoke(in request, ref response);
				}
				else
				{
					request = default;

					Server.InternalServerErrorHandler(in request, ref response);
				}

				this.WriteHandler(stream, in request, response);

				request.Dispose();

				response.Dispose();

				ranges.Dispose();

				buffer.Dispose();

				client.Dispose();
			}
			// exception gets thrown if we cancel the cancellationtoken, no need to log
			catch (SocketException) when (this.cancelToken.IsCancellationRequested)
			{
				//
			}
			catch (System.Exception ex)
			{
				this.Logger?.LogException(ex, "Exception thrown while handling client");
			}
		}

		private void WriteHandler(NetworkStream networkStream, in Request request, ResponseBuilder response)
		{
			if (request.Method is HttpMethod.Head or HttpMethod.Options)
			{
				networkStream.Write(response.GetHeaders());
				return;
			}

			var supported = this.Options.SupportedCompression;

			if (supported == CompressionMethod.None)
			{
				networkStream.Write(response.Content);
				return;
			}

			if (!request.TryGetHeader(Headers.AcceptEncoding, out var acceptEncoding) ||
				!ServerHelpers.TryGetValidCompressionMode(acceptEncoding.Value, supported, out var compression))
			{
				networkStream.Write(response.Content);
				return;
			}

			try
			{
				var stream = Compression.GetWriteStream(networkStream, compression);

				networkStream.Write(response.GetHeaders());
				stream.Write(response.GetContent());

				// we dispose the actual network stream in the TCP client
				if (compression != CompressionMethod.None)
				{
					stream.Dispose();
				}
			}
			catch (SocketException ex)
			{
				this.Logger?.LogException(ex, "Exception writing response to stream");
			}
		}

		// @todo Move to ServerHelpers?
		internal RequestHandler.RequestHandlerCallback GetRequestHandler(HttpMethod requestMethod,
																		 System.ReadOnlySpan<byte> requestPath,
																		 out RentedArray<KeyValue> parameters)
		{
			foreach (var handler in this.handlers)
			{
				if (!ServerHelpers.ComparePaths(requestPath, handler.Path, out parameters))
				{
					continue;
				}

				// OPTIONS & HEAD can ignore the handler's method
				if (requestMethod != HttpMethod.Head &&
					requestMethod != HttpMethod.Options &&
					handler.Method != requestMethod)
				{
					return Server.MethodNotAllowedHandler;
				}

				return handler.Handler;
			}

			parameters = default;
			return this.FallbackHandler;
		}

		private void FallbackHandler(in Request request, ref ResponseBuilder response)
		{
			if (this.Options.StaticFiles is null)
			{
				// @todo Customizable
				response.AppendHeaders(in request, in StatusCodes.Status404NotFound, ContentTypes.Html);
				return;
			}

			var fileInfo = StaticFiles.GetStaticFileInfo(this.Options.StaticFiles, request.Path);

			if (fileInfo is null)
			{
				// @todo Customizable
				response.AppendHeaders(in request, in StatusCodes.Status404NotFound, ContentTypes.Html);
				return;
			}

			response.AppendFile(in request, fileInfo);
		}

		// @todo Customizable
		private static void MethodNotAllowedHandler(in Request request, ref ResponseBuilder response) =>
			response.AppendHeaders(in request, in StatusCodes.Status405MethodNotAllowed, ContentTypes.Html);

		// @todo Customizable
		private static void InternalServerErrorHandler(in Request request, ref ResponseBuilder response) =>
			response.AppendHeaders(in request, in StatusCodes.Status404NotFound, ContentTypes.Html);

		public void Dispose()
		{
			this.Logger?.LogInfo("Closing server...");

			this.cancelRegistration.Dispose();

			this.listener.Dispose();
		}
	}
}
