using System.Text;
using System.Threading;
using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia.Example
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			var cancelTokenSource = new CancellationTokenSource();

			var options = new ServerOptions("0.0.0.0", 6969, new ConsoleLogger())
			{
				StaticFiles = new[] { "_static" },
			};

			var server = new Server(options, cancelTokenSource.Token);

			server.AddRazorPage<Test>("/"u8);
			server.AddRazorPage<Test>("/test"u8);

			server.AddRequestHandler(new RequestHandler("/html"u8, Program.RawHtmlHandler));
			server.AddRequestHandler(new RequestHandler("/json"u8, Program.JsonHandler));
			server.AddRequestHandler(new RequestHandler(HttpMethod.Post, "/post"u8, Program.PostHandler));
			server.AddRequestHandler(new RequestHandler("/resize"u8, Program.ResizeHandler));

			var thread = new Thread(server.Listen);

			thread.Start();
		}

		private static void RawHtmlHandler(in Request request, ref ResponseBuilder response)
		{
			var html = $"<html><body><h1>Hello from {request.Path}!</h1></html></body>";

			response.AppendHtml(in request, in StatusCodes.Status200OK, html);
		}

		private static void JsonHandler(in Request request, ref ResponseBuilder response)
		{
			var json = "{\"test\": \"Hello there!\", \"number\": 69}";

			var jsonSize = Encoding.UTF8.GetByteCount(json);

			response.AppendHeaders(in request, in StatusCodes.Status200OK, ContentTypes.Json);

			var span = response.Take(jsonSize);

			var written = Encoding.UTF8.GetBytes(json, span);

			response.Move(written);
		}

		private static void PostHandler(in Request request, ref ResponseBuilder builder)
		{
			if (MultipartFormData.TryParse(in request, out var data, out var count))
			{
				for (var i = 0; i < count; i++)
				{
					var item = data[i];

					System.Console.WriteLine(item.Name + ": " + item.Content);
				}

				data.Dispose();

				builder.AppendHeaders(in request, in StatusCodes.Status204NoContent, default);
				return;
			}

			if (Json.TryParse(in request, out Person body))
			{
				builder.AppendJson(in request, in StatusCodes.Status200OK, body);
				return;
			}

			builder.AppendHeaders(in request, in StatusCodes.Status500InternalServerError, default);
		}

		private static void ResizeHandler(in Request request, ref ResponseBuilder builder)
		{
			const int size = 10000;

			builder.AppendHeaders(in request, in StatusCodes.Status200OK, ContentTypes.Json);

			builder.Append('[');

			for (var i = 0; i < size; i++)
			{
				var person = new Person
				{
					Name = "Person " + i,
					Age = i,
				};

				builder.Append(person);

				if (i < size - 1)
				{
					builder.Append(',');
				}
			}

			builder.Append(']');
		}
	}
}
