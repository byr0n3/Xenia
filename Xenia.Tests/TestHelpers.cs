﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;
using HttpMethod = Byrone.Xenia.Data.HttpMethod;

namespace Xenia.Tests
{
	internal static class TestHelpers
	{
		public const string TestHtml = "<html><body><h1>Hello world!</h1></body></html>";
		public const string TestJson = "{\"test\": \"It seems to work!\"}";
		public const string TestRazor = "This is a razor page!";

		public static readonly Person Person = new()
		{
			Name = "John Doe",
			Age = 21,
		};

		public static Server CreateServer(CancellationToken token)
		{
			var options = new ServerOptions
			{
				IpAddress = "0.0.0.0",
				Port = ServerTests.Port,
				StaticFiles = new[] { "_static", "_cdn" },
			};

			var server = new Server(options, token);

			server.AddRequestHandler(new RequestHandler("/test"u8, Handler));
			server.AddRequestHandler(new RequestHandler("/json"u8, JsonHandler));
			server.AddRequestHandler(new RequestHandler("/json/model"u8, JsonModelHandler));
			server.AddRequestHandler(new RequestHandler("/resize"u8, TestHelpers.ResizeHandler));
			server.AddRequestHandler(new RequestHandler("/query"u8, TestHelpers.QueryParametersHandler));
			server.AddRequestHandler(new RequestHandler(HttpMethod.Post, "/post"u8, Handler));
			server.AddRequestHandler(new RequestHandler(HttpMethod.Post, "/post/formdata"u8,
														TestHelpers.PostFormDataHandler));
			server.AddRequestHandler(new RequestHandler(HttpMethod.Post, "/post/json"u8, TestHelpers.PostJsonHandler));
			server.AddRazorPage<TestPage>("/razor"u8);

			return server;

			static void Handler(in Request request, ref ResponseBuilder response) =>
				response.AppendHtml(in request, in StatusCodes.Status200OK, TestHelpers.TestHtml);

			static void JsonHandler(in Request request, ref ResponseBuilder response) =>
				response.AppendJson(in request, in StatusCodes.Status200OK,
									Encoding.UTF8.GetBytes(TestHelpers.TestJson));

			static void JsonModelHandler(in Request request, ref ResponseBuilder response) =>
				response.AppendJson(in request, in StatusCodes.Status200OK, TestHelpers.Person);
		}

		private static void PostJsonHandler(in Request request, ref ResponseBuilder response)
		{
			if (!Json.TryParse(in request, out Person model))
			{
				response.AppendHeaders(in request, in StatusCodes.Status500InternalServerError, default);
				return;
			}

			response.AppendJson(in request, in StatusCodes.Status200OK, model);
		}

		// write 10000 person instances as JSON to the response
		// if the server can't resize the buffer, the server will crash
		private static void ResizeHandler(in Request request, ref ResponseBuilder response)
		{
			const int size = 10000;

			response.AppendHeaders(in request, in StatusCodes.Status200OK, ContentTypes.Json);

			response.Append('[');

			for (var i = 0; i < size; i++)
			{
				var person = new Person
				{
					Name = "Person " + i,
					Age = i,
				};

				response.Append(person);

				if (i < size - 1)
				{
					response.Append(',');
				}
			}

			response.Append(']');
		}

		private static void PostFormDataHandler(in Request request, ref ResponseBuilder response)
		{
			if (!MultipartFormData.TryParse(in request, out var data, out var count) ||
				!data.TryFindItem(count, "name"u8, out var name) ||
				!data.TryFindItem(count, "age"u8, out var age))
			{
				response.AppendHeaders(in request, in StatusCodes.Status500InternalServerError, default);
				return;
			}

			var person = new Person
			{
				Name = name.Content.ToString() ?? string.Empty,
				Age = int.Parse(age.Content),
			};

			response.AppendHeaders(in request, in StatusCodes.Status200OK, ContentTypes.Json);

			response.Append(person);
		}

		private static void QueryParametersHandler(in Request request, ref ResponseBuilder response)
		{
			using (var @params = QueryParameters.Parse(request.Query))
			{
				if (!@params.TryGetValue("name"u8, out var name) || !@params.TryGetValue("age"u8, out var param))
				{
					response.AppendHeaders(in request, in StatusCodes.Status400BadRequest, default);

					return;
				}

				var person = new Person
				{
					Name = HttpUtility.UrlDecode(name.Value.ToString() ?? string.Empty),
					Age = int.Parse(param.Value),
				};

				response.AppendJson(in request, in StatusCodes.Status200OK, person);
			}
		}

		public static void AssertResponse(HttpResponseMessage? response,
										  HttpStatusCode code,
										  string expectedContentType)
		{
			Assert.IsNotNull(response);

			Assert.IsTrue(response.StatusCode == code);

			Assert.IsTrue(response.Content.Headers.TryGetValues("Content-Type", out var contentType));

			Assert.IsTrue(
				contentType.Any((value) => string.Equals(value, expectedContentType, System.StringComparison.Ordinal)));
		}
	}
}
