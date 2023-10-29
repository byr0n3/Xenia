using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Xenia.Tests
{
	public sealed partial class ServerTests
	{
		[TestMethod]
		public async Task ServerCanReturnRawJsonAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/json").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			Assert.IsTrue(string.Equals(html, TestHelpers.TestJson, System.StringComparison.Ordinal));

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanReturnJsonFromEntityAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/json/model").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var content = await response.Content.ReadFromJsonAsync<Person>().ConfigureAwait(false);

			Assert.IsTrue(content == TestHelpers.Person);

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanParseAndReturnJsonAsync()
		{
			var response = await
				ServerTests.httpClient
						   .PostAsJsonAsync("/post/json", TestHelpers.Person)
						   .ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var content = await response.Content.ReadFromJsonAsync<Person>().ConfigureAwait(false);

			Assert.IsTrue(content == TestHelpers.Person);

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanParseMultipartFormDataAsync()
		{
			var person = new Person
			{
				Name = "John Doe",
				Age = 21,
			};

			var requestContent = new MultipartFormDataContent();

			requestContent.Add(new StringContent(person.Name), "name");
			requestContent.Add(new StringContent(person.Age.ToString()), "age");

			var response = await ServerTests.httpClient.PostAsync("/post/formdata", requestContent);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var content = await response.Content.ReadFromJsonAsync<Person>().ConfigureAwait(false);

			Assert.IsTrue(content == person);
		}
	}
}
