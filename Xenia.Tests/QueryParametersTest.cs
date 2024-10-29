using System.Globalization;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia.Tests
{
	public sealed class QueryParametersTest
	{
		[Fact]
		public void CanFindSimpleQueryParametersFromQuery()
		{
			var @params = new QueryParameters("?hello=world&value=blog"u8);

			QueryParametersTest.AssertFound(@params, "hello"u8, "world"u8);
			QueryParametersTest.AssertFound(@params, "value"u8, "blog"u8);

			Assert.False(@params.TryGet("not-found"u8, out _));
		}

		[Fact]
		public void CanParseSimpleQueryParametersFromQuery()
		{
			var @params = new QueryParameters("?id=2&date=2024-01-01&datetime=2024-01-01T10:00:00.0000250"u8);

			QueryParametersTest.AssertFound(@params, "id"u8, 2);
			QueryParametersTest.AssertFound(
				@params,
				"date"u8,
				(Date)System.DateOnly.Parse("2024-01-01", DateTimeFormatInfo.InvariantInfo)
			);
			QueryParametersTest.AssertFound(
				@params,
				"datetime"u8,
				(DateTime)System.DateTime.Parse("2024-01-01T10:00:00.0000250", DateTimeFormatInfo.InvariantInfo)
			);

			Assert.False(@params.TryGet("not-found"u8, out _));
			Assert.False(@params.TryGet<Date>("id"u8, out _)); // Is an `int`, not a date
		}

		[Fact]
		public void CanFindSimpleQueryParametersFromPath()
		{
			var @params = QueryParameters.FromUrl("/hello/world?hello=world&value=blog"u8);

			QueryParametersTest.AssertFound(@params, "hello"u8, "world"u8);
			QueryParametersTest.AssertFound(@params, "value"u8, "blog"u8);
		}

		[Fact]
		public void CanParseSimpleQueryParametersFromPath()
		{
			var path = "/hello/world?id=2&date=2024-01-01&datetime=2024-01-01T10:00:00.0000250"u8;
			var @params = QueryParameters.FromUrl(path);

			QueryParametersTest.AssertFound(@params, "id"u8, 2);
			QueryParametersTest.AssertFound(
				@params,
				"date"u8,
				(Date)System.DateOnly.Parse("2024-01-01", DateTimeFormatInfo.InvariantInfo)
			);
			QueryParametersTest.AssertFound(
				@params,
				"datetime"u8,
				(DateTime)System.DateTime.Parse("2024-01-01T10:00:00.0000250", DateTimeFormatInfo.InvariantInfo)
			);
		}

		[Fact]
		public void CanFindSimpleQueryParametersFromUrl()
		{
			var @params = QueryParameters.FromUrl("http://localhost/hello/world?hello=world&value=blog"u8);

			QueryParametersTest.AssertFound(@params, "hello"u8, "world"u8);
			QueryParametersTest.AssertFound(@params, "value"u8, "blog"u8);
		}

		[Fact]
		public void CanParseSimpleQueryParametersFromUrl()
		{
			var url = "http://localhost/hello/world?id=2&date=2024-01-01&datetime=2024-01-01T10:00:00.0000250"u8;
			var @params = QueryParameters.FromUrl(url);

			QueryParametersTest.AssertFound(@params, "id"u8, 2);
			QueryParametersTest.AssertFound(
				@params,
				"date"u8,
				(Date)System.DateOnly.Parse("2024-01-01", DateTimeFormatInfo.InvariantInfo)
			);
			QueryParametersTest.AssertFound(
				@params,
				"datetime"u8,
				(DateTime)System.DateTime.Parse("2024-01-01T10:00:00.0000250", DateTimeFormatInfo.InvariantInfo)
			);
		}

		private static void AssertFound(QueryParameters @params,
										scoped System.ReadOnlySpan<byte> key,
										scoped System.ReadOnlySpan<byte> value)
		{
			var found = @params.TryGet(key, out var result);

			Assert.True(found);
			Assert.Equal(value, result);

			Assert.Equal(value, @params.Get(key));
		}

		private static void AssertFound<T>(QueryParameters @params, scoped System.ReadOnlySpan<byte> key, T value)
			where T : System.IUtf8SpanParsable<T>
		{
			var found = @params.TryGet<T>(key, out var result);

			Assert.True(found);
			Assert.Equal(value, result);

			Assert.Equal(value, @params.Get<T>(key));
		}
	}
}
