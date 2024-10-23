using System.Net.Http;
using System.Threading;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia.Tests
{
	/// <summary>
	/// Base class for testing <see cref="Server"/> functionalities.
	/// </summary>
	/// <remarks>
	/// <para>Sets-up a <see cref="Server"/>, creates a <see cref="Thread"/> for accepting clients and creates a <see cref="HttpClient"/> to do requests with.</para>
	/// <para>Also handles disposing/releasing these allocated resources.</para>
	/// </remarks>
	public abstract class BaseServerTests : System.IDisposable
	{
		private readonly Server server;
		private readonly Thread thread;

		protected readonly HttpClient HttpClient;

		protected BaseServerTests(ushort port)
		{
			var config = new Config(IPv4.Local, port);
			this.server = new Server(config, this.RequestHandler);

			this.thread = new Thread(this.server.Run)
			{
				IsBackground = true,
				Name = "Xenia Test Server Thread",
			};
			this.thread.Start();

			this.HttpClient = new HttpClient
			{
				BaseAddress = new System.Uri($"http://localhost:{port}/", System.UriKind.Absolute),
			};
		}

		public void Dispose()
		{
			this.server.Dispose();
			this.thread.Join();

			this.HttpClient.Dispose();

			System.GC.SuppressFinalize(this);
		}

		protected abstract IResponse RequestHandler(in Request request);
	}
}
