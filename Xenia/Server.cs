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
			this.listener = new TcpListener(ip, options.Port);

			this.listener.Start();

			this.Logger.LogInfo($"Server started on http://{options.IpAddress}:{options.Port}");
		}

		public void Listen()
		{
			while (!this.cancelToken.IsCancellationRequested)
			{
				this.HandleRequest();
			}
		}

		private void HandleRequest()
		{
			var client = this.listener.AcceptTcpClient();
			var stream = client.GetStream();

			if (!stream.CanRead)
			{
				this.Logger.LogWarning("Received an unreadable stream");

				return;
			}

			using (var buffer = new RentedArray<byte>(Server.bufferSize))
			{
				var read = stream.Read(buffer.AsSpan());
				
				// @todo Handle not everything read

				if (read == 0)
				{
					this.Logger.LogWarning("No data read");

					return;
				}

				var bytes = buffer.AsSpan(0, read);

				using (var ranges = new RentedArray<System.Range>(15)) // @todo
				{
					var count = bytes.Split(ranges.Data, (byte)'\n');

					if (count == 0 || !ServerHelpers.TryGetRequest(bytes, ranges.AsSpan(0, count), out var request))
					{
						this.Logger.LogWarning("Unable to parse request");

						return;
					}

					// @todo Request handlers

					// @todo Handle not found/method not allowed

					request.Dispose();
				}
			}
		}

		public void Dispose()
		{
			this.Logger.LogInfo("Closing server...");

			this.listener.Dispose();
		}
	}
}
