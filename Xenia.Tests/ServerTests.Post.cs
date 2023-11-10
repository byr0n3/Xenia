using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;
using HttpMethod = Byrone.Xenia.Data.HttpMethod;

namespace Xenia.Tests
{
	public sealed partial class ServerTests
	{
		[TestMethod]
		public async Task ServerCanParseAndReturnJsonAsync()
		{
			Assert.IsNotNull(ServerTests.server);

			var handler = new RequestHandler(HttpMethod.Post, "/post/json"u8, Handler);

			ServerTests.server.AddRequestHandler(in handler);

			var response = await
				ServerTests.httpClient
						   .PostAsJsonAsync("/post/json", TestHelpers.Person)
						   .ConfigureAwait(false);

			response.Dispose();

			Assert.IsTrue(ServerTests.server.RemoveRequestHandler(in handler));

			return;

			static void Handler(in Request request, ref ResponseBuilder builder)
			{
				Assert.IsTrue(Json.TryParse(in request, out Person model));

				Assert.IsTrue(model == TestHelpers.Person);

				builder.AppendHeaders(in request, in StatusCodes.Status200OK, default);
			}
		}

		[TestMethod]
		public async Task ServerCanParseMultipartFormDataAsync()
		{
			Assert.IsNotNull(ServerTests.server);

			var person = new Person
			{
				Name = "John Doe",
				Age = 21,
			};

			var handler = new RequestHandler(HttpMethod.Post, "/post/formdata"u8, Handler);

			ServerTests.server.AddRequestHandler(in handler);

			var requestContent = new MultipartFormDataContent();

			requestContent.Add(new StringContent(person.Name), "name");
			requestContent.Add(new StringContent(person.Age.ToString()), "age");

			var response = await ServerTests.httpClient.PostAsync("/post/formdata", requestContent);

			requestContent.Dispose();

			response.Dispose();

			Assert.IsTrue(ServerTests.server.RemoveRequestHandler(in handler));

			return;

			void Handler(in Request request, ref ResponseBuilder builder)
			{
				Assert.IsTrue(MultipartFormData.TryParse(in request, out var data, out var count));
				Assert.IsTrue(data.TryFindItem(count, "name"u8, out var name));
				Assert.IsTrue(data.TryFindItem(count, "age"u8, out var age));

				var requestPerson = new Person
				{
					Name = name.Content.ToString() ?? string.Empty,
					Age = int.Parse(age.Content),
				};

				Assert.IsTrue(requestPerson == person);

				builder.AppendHeaders(in request, in StatusCodes.Status200OK, default);
			}
		}
	}
}
