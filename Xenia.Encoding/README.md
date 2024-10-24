# Xenia.Encoding

Helpers to find the preferred encoding from the request and encode data.

## Usage

```csharp
using Byrone.Xenia;
using Byrone.Xenia.Utilities;

static IResponse RequestHandler(in Request request) =>
	new Response();

private readonly struct Response : IResponse
{
	public void Send(Socket client, in Request request)
	{
		var body = "<html><body>Hello World</body></html>"u8;

		if (request.TryGetEncoding(out var encoding)) 
		{
			var stream = new RentedMemoryStream(64);
	
			var compressed = BodyEncoding.TryEncode(stream, encoding, EncodingTests.Body);
	
			Debug.Assert(compressed);
	
			var response = StringBuilder.Format(
				stackalloc byte[128],
				$"""
	 			HTTP/1.1 200 OK
	 			Content-Encoding: {encoding}
	 			Content-Type: text/html
	 			Server: xenia-test-server
	
	
	 			""" 
			);
	
			client.Send(response);
			client.Send(System.MemoryExtensions.AsSpan(stream.GetBuffer(), 0, (int)stream.Position));
	
			stream.Dispose();
		}
		else
		{
			// Handle fallback...    
		}
	}
}
```
