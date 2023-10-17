using System.Text;
using System.Threading;

namespace Byrone.Xenia.Example
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			var cancelTokenSource = new CancellationTokenSource();

			var options = new ServerOptions
			{
				IpAddress = "0.0.0.0",
				Port = 6969,
				Logger = new ConsoleLogger(),
			};

			var server = new Server(options, cancelTokenSource.Token);

			server.RegisterHandler(new RequestHandler("/"u8, Program.SimpleHandler));
			server.RegisterHandler(new RequestHandler("/test"u8, Program.TestHandler));
			server.RegisterHandler(new RequestHandler(HttpMethod.Post, "/post"u8, Program.SimpleHandler));

			var thread = new Thread(server.Listen);

			thread.Start();
		}

		private static void SimpleHandler(ResponseWriter writer, in Request request)
		{
			System.Console.WriteLine($"Hello from {Encoding.UTF8.GetString(request.Path)}!");
		}

		private static void TestHandler(ResponseWriter writer, in Request _)
		{
			System.Console.WriteLine("Test handler!!");
		}
	}
}
