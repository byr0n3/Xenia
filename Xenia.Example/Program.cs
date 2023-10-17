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

			var thread = new Thread(server.Listen);

			thread.Start();
		}
	}
}
