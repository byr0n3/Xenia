# Xenia.Caching

Utilities for working with cached-content and applying caching headers.

## Usage

```csharp
using Byrone.Xenia;
using Byrone.Xenia.Utilities;

private readonly struct Response : IResponse
{
    public void Send(Socket client, in Request request)
    {
		System.Span<byte> buffer = stackalloc byte[128];

		var cacheable = new Cacheable(CacheType.Public, (long)System.TimeSpan.FromHours(1).TotalSeconds, "tag"u8);

		var cacheHeaders = Cache.GetCacheHeaders(buffer, cacheable);

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
}
```
