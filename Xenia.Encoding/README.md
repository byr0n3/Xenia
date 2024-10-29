# Xenia.Encoding

Helpers to find the preferred encoding from the request and encode data.

## Usage

A request may send the `Accept-Encoding` header. The value of this header is a list of accepted encodings, each encoding
with its own weight defining the preference of the browser. By installing `Xenia.Encoding`, you can easily handle
finding the preferred encoding and using this encoding to encode your response. Please keep in mind that **only** the
response **body** should be encoded, **not the headers**.

Here's an example on how to get the right encoding and encode the response data:

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

		// Parse the `Accept-Encoding` header, if present
		if (request.TryGetEncoding(out var encoding)) 
		{
			// Create a temporary stream to write encoded data to
			var stream = new RentedMemoryStream(64);
	
			// Encode the data to the stream and return a span containing the encoded data 
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
			// Send the encoded data
			client.Send(System.MemoryExtensions.AsSpan(stream.GetBuffer(), 0, (int)stream.Position));
	
			// Don't forget to dispose and release the stream's resources!
			stream.Dispose();
		}
		else
		{
			// Handle fallback...    
		}
	}
}
```
