# Xenia.Encoding

Helpers to find the preferred encoding from the request and encode data.

## Usage

```csharp
static IResponse RequestHandler(in Request request)
{
	var body = "<html><body>Hello World</body></html>"u8;
    
	var stream = new MemoryStream(64);

	// Try to find a/the preferred encoding from the request.
	// If an encoding has been found, try to encode the input data and write it to the stream. 
	if (request.TryGetEncoding(out var encoding) && 
	    BodyEncoding.TryEncode(stream, encoding, body)) 
	{
		var compressed = BodyEncoding.TryEncode(stream, encoding, body);
	}
	// Fallback, in case no supported encoding was found. 
	else
	{
		stream.Write(body);
	}

	return new Response(stream, encoding);
}

private readonly struct Response : IResponse
{
	private readonly MemoryStream stream;
	private readonly Unmanaged encoding;

	public Response(MemoryStream stream, System.ReadOnlySpan<byte> encoding)
	{
		this.stream = stream;
		this.encoding = encoding;
	}

	public void Send(Socket client)
	{
		// Base HTTP response, note the `Content-Encoding` header.
		var response = StringBuilder.Format(
			stackalloc byte[128], 
			$"""
			HTTP/1.1 200 OK
			Content-Encoding: {this.encoding}
			Content-Type: text/html

			""" 
		);

		// Take the slice of the stream that has been written to.
		var body = System.MemoryExtensions.AsSpan(
			this.stream.GetBuffer(),
			0,
			(int)this.stream.Position
		);

		client.Send(response); 
		client.Send(body); 
	} 
}
```
