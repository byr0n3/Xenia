# Xenia.Caching

Utilities for working with cached-content and applying caching headers.

## Usage

Caching content with HTTP has always been a tricky process. There's a lot of different headers that are needed to be
validated and compared in order to cache content efficiently. The `Xenia.Caching` package tries to make this as easy as
possible.

Using this package (and this documentation) assumes you have a basic understanding of how HTTP caching works. If you
don't, or would like to find some more information, you
can [read more here](https://developer.mozilla.org/en-US/docs/Web/HTTP/Caching).

Keep in mind that this package only handles creating headers that instruct the browser how the response is allowed to be
cached, when it should be revalidated and if the current content is stale (in need of validation). You are still in
charge of assigning what content should be cached how and when.

This package also doesn't provide any functionality to cache data on the server, like in a Redis store.

Here's some basic examples on how to cache responses:

```csharp
using Byrone.Xenia;
using Byrone.Xenia.Utilities;

private readonly struct Response : IResponse
{
    public void Send(Socket client, in Request request)
    {
		System.Span<byte> buffer = stackalloc byte[128];

		// A 'cacheable' is a declaration of how content is allowed to be cached. This example:
		// - should be stored in public cache (machine-wide, allowed to be used for multiple users)
		// - should only be stored for one hour
		// - has an ETag of 'tag' (your ETag should be quoted!)
		var cacheable = new Cacheable(
			CacheType.Public,
			(long)System.TimeSpan.FromHours(1).TotalSeconds,
			"\"tag\""u8
		);

		// You can also declare your response as uncachable like this:
		var cacheable = Cacheable.NoCache;

		// Transform your cacheable into the correct cache headers
		var cacheHeaders = Cache.GetCacheHeaders(buffer, cacheable);

		// Check if the request has any headers declaring it's cache state and if it should revalidate the cache
		if (Cache.IsStale(in request, cacheable)) 
		{
			// When this function returns `true`, your server should fetch/return the response
			var response = StringBuilder.Format(
				stackalloc byte[256],
				$"""
				HTTP/1.1 200 OK
				{cacheHeaders}
				Content-Type: text/html
				Server: xenia-server

				<html><body><h1>This response should be cached by the browser.</h1></body></html>
				"""
			);

			client.Send(response);
		}
		else
		{
			// When this function returns `false`, the browser's cache is still valid, no need to fetch/send the response again.
			var response = StringBuilder.Format(
				stackalloc byte[256],
				$"""
				HTTP/1.1 304 Not Modified
				{cacheHeaders}
				Content-Type: text/html
				Server: xenia-server


				"""
			);

			client.Send(response);
		}
	}
}
```

Caching is in this example very useless, as the response body is quite small and doesn't take much resources/time to
calculate and send. However, HTTP caching can be a powerful tool for optimizing your server. For example, when you have
an endpoint that returns a very heavy calculation, that only changes every x amount of time, you should tell the
client's browser to cache the response for a while.

### Defining cacheables

A cacheable has multiple properties that help you define how to cache your content.

| Property     | Description                                                                                                             | Allowed values                 | Example                                              |   |
|--------------|-------------------------------------------------------------------------------------------------------------------------|--------------------------------|------------------------------------------------------|---|
| Type         | The type of cache to use. See the in-code docs for more info.                                                           | `NoCache`, `Public`, `Private` | `CacheType.Public`                                   |   |
| Age          | How long (in seconds) the browser can save this response before it needs revalidation.                                  | 0 – `long.MaxValue`            | `60 * 60` (1 hour)                                   |   |
| ETag         | Identifier for a specific version of the resource. This could be a (file)version, but also a hash of the response body. | Any valid UTF-8 string         | `"tag-here"`, `"v0.1.0"`                             |   |
| Vary         | Which headers of the response can vary between cached responses.                                                        | Any valid UTF-8 string         | `"Accept-Language"u8`, `"Accept-Language, Accept"u8` |   |
| LastModified | The date and time when the server believes the resource was last modified.                                              | A `System.DateTime` value      | `System.DateTime.UtcNow`                             |   |
