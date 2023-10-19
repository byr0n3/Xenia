using System.Text;
using System.Threading;
using Byrone.Xenia.Extensions;

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
			server.RegisterHandler(new RequestHandler("/test"u8, Program.SimpleHandler));
			server.RegisterHandler(new RequestHandler("/json"u8, Program.JsonHandler));
			server.RegisterHandler(new RequestHandler(HttpMethod.Post, "/post"u8, Program.SimpleHandler));

			var thread = new Thread(server.Listen);

			thread.Start();
		}

		private static void SimpleHandler(in Request request, ref ResponseBuilder response)
		{
			var html = $"<html><body><h1>Hello from {Encoding.UTF8.GetString(request.Path)}!</h1></html></body>";

			response.AppendHtml(in request, StatusCodes.Status200OK, html);
		}

		private static void JsonHandler(in Request request, ref ResponseBuilder response)
		{
			var json = "{\"test\": \"Hello there!\", \"number\": 69}";

			var jsonSize = Encoding.UTF8.GetByteCount(json);

			response.AppendHeaders(in request, StatusCodes.Status200OK, "application/json"u8, jsonSize);

			var span = response.Take(jsonSize);

			var written = Encoding.UTF8.GetBytes(json, span);

			response.Move(written);
		}
	}
}
