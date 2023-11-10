using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;
using Byrone.Xenia.Internal;

namespace Byrone.Xenia
{
	public sealed class Server : System.IDisposable
	{
		internal readonly ServerOptions Options;

		private readonly Socket socket;
		private readonly IPEndPoint endPoint;
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
			this.endPoint = new IPEndPoint(ip, options.Port);
			this.socket = new Socket(this.endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			this.cancelRegistration = this.cancelToken.Register(this.Dispose);
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
			this.socket.Bind(this.endPoint);
			this.socket.Listen(int.MaxValue); // @todo Configurable

			if (this.ShouldLog(LogLevel.Info))
			{
				this.Logger?.LogInfo(
					$"Server started listening on: on http://{this.Options.IpAddress}:{this.Options.Port}");
			}

			while (!this.cancelToken.IsCancellationRequested)
			{
				this.HandleConnection();
			}
		}

		private void HandleConnection()
		{
			try
			{
				var client = this.socket.Accept();

				var timestamp = System.DateTime.UtcNow.Ticks;

				// @todo ResizableRentedArray
				var buffer = new RentedArray<byte>(this.Options.GetBufferSize());
				var read = this.ReadBytes(client, buffer);

				if (read <= 0)
				{
					client.Dispose();
					buffer.Dispose();

					return;
				}

				var bytes = buffer.AsSpan().Slice(0, read);

				this.AppendResponse(client, bytes, timestamp);

				// we're done writing, we can close the connection to the client and clean up now

				client.Dispose();

				buffer.Dispose();
			}
			catch (SocketException ex)
			{
				if (!this.cancelToken.IsCancellationRequested)
				{
					this.Logger?.LogException(ex, "Socket exception thrown.");
				}
			}
			catch (System.Exception ex)
			{
				if (this.ShouldLog(LogLevel.Exceptions))
				{
					this.Logger?.LogException(ex, "Exception thrown while handling client.");
				}
			}
		}

		private int ReadBytes(Socket client, RentedArray<byte> buffer)
		{
			client.ReceiveTimeout = 500;

			try
			{
				return client.Receive(buffer.AsSpan(), SocketFlags.None);
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
			{
				// empty request/connection error
				return 0;
			}
			catch (SocketException ex)
			{
				if (this.ShouldLog(LogLevel.Error))
				{
					this.Logger?.LogException(ex, "Receiving bytes from client threw an exception");
				}

				return 0;
			}
		}

		private void AppendResponse(Socket client, System.ReadOnlySpan<byte> bytes, long timestamp)
		{
			// @todo Resizable?
			System.Span<System.Range> ranges = stackalloc System.Range[32];

			var count = bytes.Split(ranges, Characters.NewLine);

			Debug.Assert(count != 0);

			// @todo Immediately write to socket instead of to array?
			var response = new ResponseBuilder();

			if (ServerHelpers.TryGetRequest(this, bytes, ranges.Slice(0, count), out var request))
			{
				request.HandlerCallback.Invoke(in request, ref response);
			}
			else
			{
				request = default;

				Server.InternalServerErrorHandler(in request, ref response);
			}

			this.WriteHandler(client, in request, response);

			this.LogElapsed(request.Path, timestamp);

			request.Dispose();

			response.Dispose();
		}

		private void WriteHandler(Socket client, in Request request, ResponseBuilder response)
		{
			if (request.Method is HttpMethod.Head or HttpMethod.Options)
			{
				client.Send(response.GetHeaders());
				return;
			}

			var supported = this.Options.SupportedCompression;

			if (supported == CompressionMethod.None)
			{
				client.Send(response.Content);
				return;
			}

			if (!request.TryGetHeader(Headers.AcceptEncoding, out var acceptEncoding) ||
				!ServerHelpers.TryGetValidCompressionMode(acceptEncoding.Value, supported, out var compression))
			{
				client.Send(response.Content);
				return;
			}

			try
			{
				var stream = Compression.GetWriteStream(client, compression);

				client.Send(response.GetHeaders());
				stream.Write(response.GetContent());

				stream.Dispose();
			}
			catch (SocketException ex)
			{
				if (this.ShouldLog(LogLevel.Exceptions))
				{
					this.Logger?.LogException(ex, "Exception writing response to stream");
				}
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
			var fileInfo = StaticFiles.GetStaticFileInfo(this.Options.StaticFiles, request.Path);

			if (fileInfo is not null)
			{
				response.AppendFile(in request, fileInfo);
				return;
			}

			// @todo Customizable
			response.AppendHeaders(in request, in StatusCodes.Status404NotFound, ContentTypes.Html);
		}

		// @todo Customizable
		private static void MethodNotAllowedHandler(in Request request, ref ResponseBuilder response) =>
			response.AppendHeaders(in request, in StatusCodes.Status405MethodNotAllowed, ContentTypes.Html);

		// @todo Customizable
		private static void InternalServerErrorHandler(in Request request, ref ResponseBuilder response) =>
			response.AppendHeaders(in request, in StatusCodes.Status404NotFound, ContentTypes.Html);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void LogElapsed(BytePointer path, long timestamp)
		{
			if (!this.ShouldLog(LogLevel.Info))
			{
				return;
			}

			var elapsed = (System.DateTime.UtcNow.Ticks - timestamp);
			var scale = elapsed >= System.TimeSpan.TicksPerMillisecond
				? System.TimeSpan.TicksPerMillisecond
				: System.TimeSpan.TicksPerMicrosecond;
			var suffix = elapsed >= System.TimeSpan.TicksPerMillisecond ? "ms" : "μs";

			this.Logger?.LogInfo($"Handled {path.ToString()} in {elapsed / scale}{suffix}");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool ShouldLog(LogLevel level) =>
			(this.Options.LogLevel & level) != LogLevel.None && this.Logger is not null;

		public void Dispose()
		{
			if (this.ShouldLog(LogLevel.Info))
			{
				this.Logger?.LogInfo("Closing server...");
			}

			// disposable logger support.
			// ReSharper disable once SuspiciousTypeConversion.Global
			if (this.Logger is System.IDisposable logger)
			{
				logger.Dispose();
			}

			this.cancelRegistration.Dispose();

			this.socket.Dispose();
		}
	}
}
