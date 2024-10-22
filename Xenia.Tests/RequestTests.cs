using Byrone.Xenia.Internal;

namespace Byrone.Xenia.Tests
{
	public sealed class RequestTests
	{
		[Fact]
		public void CanParseGetRequests()
		{
			var data = "GET / HTTP/1.1\nHost: test.xenia\nUser-Agent: Xenia-Test\n\n"u8;

			var parsed = RequestParser.TryParse(data, out var request);

			Assert.True(parsed);

			Assert.Equal("GET"u8, request.Method);
			Assert.Equal("/"u8, request.Path);
			Assert.Equal("HTTP/1.1"u8, request.HttpVersion);

			Assert.Equal(2, request.Headers.Length);

			Assert.Equal("Host"u8, request.Headers[0].Key.Managed);
			Assert.Equal("test.xenia"u8, request.Headers[0].Value.Managed);

			Assert.Equal("User-Agent"u8, request.Headers[1].Key.Managed);
			Assert.Equal("Xenia-Test"u8, request.Headers[1].Value.Managed);

			Assert.Equal(default, request.Body);
		}

		[Fact]
		public void CanParsePostRequests()
		{
			var data =
				"POST /form HTTP/1.1\nHost: test.xenia\nUser-Agent: Xenia-Test\nContent-Type: application\\json\n\n{\"data\": \"Hello world!\"}"u8;

			var parsed = RequestParser.TryParse(data, out var request);

			Assert.True(parsed);

			Assert.Equal("POST"u8, request.Method);
			Assert.Equal("/form"u8, request.Path);
			Assert.Equal("HTTP/1.1"u8, request.HttpVersion);

			Assert.Equal(3, request.Headers.Length);

			Assert.Equal("Host"u8, request.Headers[0].Key.Managed);
			Assert.Equal("test.xenia"u8, request.Headers[0].Value.Managed);

			Assert.Equal("User-Agent"u8, request.Headers[1].Key.Managed);
			Assert.Equal("Xenia-Test"u8, request.Headers[1].Value.Managed);

			Assert.Equal("Content-Type"u8, request.Headers[2].Key.Managed);
			Assert.Equal("application\\json"u8, request.Headers[2].Value.Managed);

			Assert.Equal("{\"data\": \"Hello world!\"}"u8, request.Body);
		}
	}
}
