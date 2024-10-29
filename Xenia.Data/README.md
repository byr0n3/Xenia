# Xenia.Data

Collection of helpers/utilities for working with different kinds of data.

## Chunked data

Also known as `Transfer-Encoding: chunked`, this is a way of sending data when the sender is not quite sure what the
content length is yet.

The idea behind this is that data is sent in 'chunks' (hence the `chunked` directive). A chunk looks something like
this:

[//]: # (I know this isn't an .env file format, but it provides some syntax highlighting which makes this easier to read)

```dotenv
1D # A hex value representing the chunk's size (in this example, 29).
This is some chunked content. # The chunks content
```

When a request's (or response, but mostly requests use this) `Transfer-Encoding` header is set to `chunked`, the request
body is a list of chunks like this. At the end of the list, there's an empty list with a length of `0` and empty
contents. This marks the end of the content and tells the server it can stop parsing.

You can handle this in your code as followed:

```csharp
static IResponse RequestHandler(in Request request) 
{
	if (ChunkedData.HasChunkedBody(in request))
	{
		var size = ChunkedData.GetSize(request.Body);

		// You should handle this properly!
		Debug.Assert(size > 0);

		System.Span<byte> buffer = stackalloc byte[size];

		var written = ChunkedData.Parse(request.Body, buffer);

		var body = buffer.SliceUnsafe(0, written);
	}
}
```

In this example, the `body` variable will contain all the chunk content's glued together.

However, chunked content can be encoded, just like how the server's response can be encoded (
see [Xenia.Encoding](../Xenia.Encoding/README.md) for more info). You can decode the content like this (you need to
install the [Xenia.Encoding](../Xenia.Encoding/README.md) package for this to work!):

```csharp
// ...

var encoding = ChunkedData.GetChunkEncoding(in request);

System.Span<byte> dst = stackalloc byte[128];

// `data` comes from the previous example.
var decoded = BodyEncoding.TryDecode(dst, data, encoding, out var written);

Debug.Assert(decoded);

var body = dst.SliceUnsafe(0, written);
```

When decoding encoded content, keep in mind that the decoded output can have a bigger size/length than the encoded
input.

## `multipart/form-data`

`multipart/form-data` is one of the different ways that's used to format a request's body, for example when you send a
POST form.

When a request with this data type is sent, the `Content-Type` header will look something like this:

```
multipart/form-data; boundary=--BROWSER-GENERATED-NAME
```

The `boundary` here declares which parts/chunks of the request body belong to the form data. The request body contains
different chunks, following this format:

[//]: # (I know this isn't an .env file format, but it provides some syntax highlighting which makes this easier to read)

```dotenv
--BROWSER-GENERATED-NAME
content-disposition: form-data; name="example-data"
content-type: text/plain; # This header is optional. The spec defines that if this header is missing, the Content-Type will be: text/plain

Hello world! This data has been sent using the 'multipart/form-data' format.
--BROWSER-GENERATED-NAME
```

You can handle this data easily in your request handler like this:

```csharp
static IResponse RequestHandler(in Request request) 
{
	// Validates and parses the `Content-Type` header, so you can easily access the sent multipart data. 
	var multipart = Multipart.FromRequest(in request);

	// Make sure to check the `IsValid` boolean before trying to access the multipart data,
	// as the request may not be valid multipart data.
	if (multipart.IsValid)
	{
		var found = multipart.TryGet("username"u8, out var username);
		found = multipart.TryGet("password"u8, out var password);
		found = multipart.TryGet("file"u8, out var file);

		// Make sure to cleanup the allocated resources! 
		username.Dispose();
		password.Dispose();
		file.Dispose();
	}
}
```
