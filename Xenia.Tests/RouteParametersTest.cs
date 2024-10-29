using Byrone.Xenia.Utilities;

namespace Byrone.Xenia.Tests
{
	public sealed class RouteParametersTest
	{
		[Fact]
		public void CanFindRouteParameters()
		{
			var pattern = "/blog/{post}"u8;
			var path = "/blog/my-first-post"u8;

			var @params = new RouteParameters(pattern, path);

			RouteParametersTest.AssertFound(@params, "post"u8, "my-first-post"u8);
			Assert.False(@params.TryGet("not-found"u8, out _));

			pattern = "/blog/{post}/edit/{user}"u8;
			path = "/blog/my-first-post/edit/admin"u8;

			@params = new RouteParameters(pattern, path);

			RouteParametersTest.AssertFound(@params, "post"u8, "my-first-post"u8);
			RouteParametersTest.AssertFound(@params, "user"u8, "admin"u8);
			Assert.False(@params.TryGet("not-found"u8, out _));
		}

		[Fact]
		public void CanParseRouteParameters()
		{
			var pattern = "/blog/{post}"u8;
			var path = "/blog/1"u8;

			var @params = new RouteParameters(pattern, path);

			RouteParametersTest.AssertFound(@params, "post"u8, 1);
			Assert.False(@params.TryGet("not-found"u8, out _));

			pattern = "/blog/{post}/{date}"u8;
			path = "/blog/1/2024-01-01"u8;

			@params = new RouteParameters(pattern, path);

			RouteParametersTest.AssertFound(@params, "post"u8, 1);
			RouteParametersTest.AssertFound(@params, "date"u8, new Date(2024, 1, 1));

			Assert.False(@params.TryGet("not-found"u8, out _));
			Assert.False(@params.TryGet<Date>("post"u8, out _)); // Is an `int`, not a date
		}

		private static void AssertFound(RouteParameters @params,
										scoped System.ReadOnlySpan<byte> key,
										scoped System.ReadOnlySpan<byte> expected)
		{
			var found = @params.TryGet(key, out var value);

			Assert.True(found);
			Assert.Equal(expected, value);
		}

		private static void AssertFound<T>(RouteParameters @params, scoped System.ReadOnlySpan<byte> key, T expected)
			where T : System.IUtf8SpanParsable<T>
		{
			var found = @params.TryGet<T>(key, out var value);

			Assert.True(found);
			Assert.Equal(expected, value);
		}
	}
}
