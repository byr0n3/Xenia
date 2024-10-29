using Byrone.Xenia.Utilities;

namespace Byrone.Xenia.Tests
{
	public sealed class RouteCollectionTests
	{
		[Fact]
		public void CanMatchSimpleRoute()
		{
			var routes = new RouteCollection([
				"/"u8,
				"/about"u8,
				"/contact"u8,
				"/privacy-policy"u8,
			]);

			RouteCollectionTests.AssertMatch(routes, "/"u8);
			RouteCollectionTests.AssertMatch(routes, "/about"u8);
			RouteCollectionTests.AssertMatch(routes, "/contact"u8);
			RouteCollectionTests.AssertMatch(routes, "/privacy-policy"u8);

			Assert.False(routes.TryFind("/not-found"u8, out _));
			Assert.False(routes.TryFind("/about/projects"u8, out _));
		}

		[Fact]
		public void CanMatchRouteWithParameters()
		{
			var routes = new RouteCollection([
				"/"u8,
				"/blog"u8,
				"/blog/{post}"u8,
				"/blog/{post}/edit"u8,
				"/blog/{post}/edit/{user}"u8,
			]);

			RouteCollectionTests.AssertMatch(routes, "/"u8);
			RouteCollectionTests.AssertMatch(routes, "/blog"u8);
			RouteCollectionTests.AssertMatch(routes, "/blog/my-first-post"u8, "/blog/{post}"u8);
			RouteCollectionTests.AssertMatch(routes, "/blog/my-first-post/edit"u8, "/blog/{post}/edit"u8);
			RouteCollectionTests.AssertMatch(routes, "/blog/my-first-post/edit/admin"u8, "/blog/{post}/edit/{user}"u8);

			Assert.False(routes.TryFind("/not-found"u8, out _));
			Assert.False(routes.TryFind("/blog/my-first-post/delete"u8, out _));
		}

		[Fact]
		public void CanMatchRouteWithQueryParameters()
		{
			var routes = new RouteCollection([
				"/search"u8,
			]);

			RouteCollectionTests.AssertMatch(routes, "/search?page=1&per_page=10"u8, "/search"u8);
		}

		private static void AssertMatch(RouteCollection routes, scoped System.ReadOnlySpan<byte> path) =>
			RouteCollectionTests.AssertMatch(routes, path, path);

		private static void AssertMatch(RouteCollection routes,
										scoped System.ReadOnlySpan<byte> path,
										scoped System.ReadOnlySpan<byte> expectedPattern)
		{
			var found = routes.TryFind(path, out var pattern);

			Assert.True(found);
			Assert.Equal(expectedPattern, pattern);
		}
	}
}
