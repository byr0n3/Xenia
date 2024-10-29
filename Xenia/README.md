# Xenia

<img src="../assets/logo.png" alt="Xenia Logo">

A simple, minimalistic HTTP/1.1 server written in C#.

## Quickstart

```csharp
using System.Threading;
using System.Net.Sockets;
using Byrone.Xenia;
using Byrone.Xenia.Utilities;

// Binds to: 0.0.0.0:6969
var config = new Config(IPv4.Local, 6969);

var server = new Server(config, RequestHandler);

// Create a background thread to accept clients on.
var thread = new Thread(server.Run)
{
	IsBackground = true,
	Name = "Xenia Server Thread",
};
thread.Start();

while (true)
{
	if (System.Console.ReadKey().Key == System.ConsoleKey.C)
	{
		break;
	}
}

// It's important to clean up and release your resources in this exact order:
// 1. Dispose the server. This will stop the server from accepting new clients.
// 2. 'Join' the thread (terminate the background thread).
// Joining a thread is a blocking call, and not disposing the server before joining the thread...
// ...will result in the thread waiting for a new client, and THEN stopping the thread.

server.Dispose();

thread.Join();

return;

// This callback gets called everytime a new request has been received and parsed.
// It requires you to return a model that implements `IResponse`, that will write a response to the connected client's stream.
static IResponse RequestHandler(in Request request)
{
	return new Response();
}

// Xenia will automatically dispose your custom repsonse,
// if you implement the `System.IDisposable` interface.
internal readonly struct Response : IResponse
{
	public void Send(Socket client, in Request request)
	{ 
		// Send a basic HTTP and HTML response
		var response = """ 
				HTTP/1.1 200 OK
				Content-Type: text/html

				<html><body><h1>Hello world!</h1></body></html>
				"""u8;

		client.Send(response);
	}
}
```

## How Xenia works

Xenia only handles accepting clients and receiving + parsing the HTTP request the accepted client sends. You as the
developer are in control over how you handle responding to the client. You do this by implementing the `IResponse`
interface. This interface requires you to implement a `public void Send(Socket client, in Request request)` function.
Here you can send an HTTP response in whichever way you like.

Only have a few routes that you want to handle? A simple `if-statement` would probably be enough, no need to implement
some specific and overcomplicated routing solution. Have a lot of (dynamic) routes? Using the `RouteCollection` and
`RouteParameters` structs you can easily implement your own advanced routing system.

### What about `async`?

Xenia attempts to use as little `async/await` as possible, as C#'s async system has quite some overhead. This also
forces you to use this pattern, even if you don't want to. This is why (currently) Xenia is completely synchronous. It
is up to you to decide how you'd like to handle things like threading and parallelism.

Another downside of the `async/await` system is the lack of proper `System.Span<T>` and `System.ReadOnlySpan<T>`
support. While this is definitely getting better since .NET 9, we're still not quite there yet, and possibly won't be,
as Spans can be (and most of the time are) allocated on the HEAP, and since `async` methods are ran on a different
thread, possibly at a different time, there's a good chance this data is already gone.

## Utilities

### `RouteColletion`

The `RouteCollection` struct contains a list of predefined routes that you'll want to handle. It also provides a helper
function to see if a requested path matches a predefined route.

```csharp
using Byrone.Xenia;
using Byrone.Xenia.Utilities;

static IResponse RequestHandler(in Request request)
{
	var routes = new RouteCollection([
		"/"u8,
		"/blog"u8,
		"/blog/{post}"u8, // Matches with for example `/blog/my-first-post`
	]);

	if (routes.TryFind(request.Path, out var pattern))
	{
		// Send back some data based on the route by, for example,
		// calling a callback function based on the pattern
	} else {
		// No matching pattern found, show 404
	}
}
```

### `RouteParameters`

By giving this struct a `pattern` and `path`, you can retrieve dynamic variables based on the requested path.

```csharp
using Byrone.Xenia;
using Byrone.Xenia.Utilities;

static IResponse RequestHandler(in Request request)
{
	var routes = new RouteCollection([
		"/blog/{post}"u8,
	]);

	if (routes.TryFind(request.Path, out var pattern))
	{
		// Pattern will be: /blog/{post}
		// Path is: /blog/my-first-post

		var @params = new RouteParameters(pattern, request.Path);
		var post = @params.Get("post"u8); // Will return: my-first-post
		var id = @params.Get<int>("post"u8); // You can also parse parameters to value types
	} else {
		// No matching pattern found, show 404
	}
}
```

### `QueryParameters`

Sometimes, you'd like to use parameters using a query string. Xenia also has a helper struct for this.

```csharp
using Byrone.Xenia;
using Byrone.Xenia.Utilities;

static IResponse RequestHandler(in Request request)
{
	// Assuming the request path is something like: /page?id=1&action=edit
	var @params = QueryParameters.FromUrl(request.Path);

	var id = @params.Get<int>("id"u8); // Will return: 1
	var action = @params.Get("action"u8); // Will return: edit
}
```

### Parsing parameters

Both the `RouteParameters` and `QueryParameters` can automatically parse the parameter's value into a specified type (
this includes custom types). This type must implement `System.IUtf8SpanParsable<T>`, however.

Most built-in (value) types implement this interface, however not every type does. Both `System.DateOnly` and
`System.DateTime` are examples of types that don't implement this interface, mainly due to the wide variety of different
formats these types can be serialized as. Xenia provides the `Date` and `DateTime` structs in the
`Byrone.Xenia.Utilities` namespace that function exactly like the built-in `System.DateOnly` and `System.DateTime`
types, but implement the `System.IUtf8SpanParsable<T>` interface so you can use date(times) in your parameters.

```csharp
using Byrone.Xenia.Utilities;

// ...

var @params = new QueryParameters(query);

var createdAt = @params.Get<DateTime>("created_at"u8);
var birthDay = @params.Get<Date>("birth_day"u8);
```

### Logging

Xenia is able to log informational output. As everyone likes to log information differently, and not everything, you
have to implement your own logging solution. This is not required, however. Here's a simple example that logs to the
standard C# console:

```csharp
using Byrone.Xenia;

internal sealed class ConsoleLogger : ILogger
{
	public void LogDebug(scoped System.ReadOnlySpan<byte> message)
	{
		System.Console.Out.Write("[Server] DEBUG: ");
		System.Console.Out.WriteLine(ConsoleLogger.Str(message));
	}

	public void LogInfo(scoped System.ReadOnlySpan<byte> message)
	{
		System.Console.Out.Write("[Server] INFO: ");
		System.Console.Out.WriteLine(ConsoleLogger.Str(message));
	}

	public void LogWarning(scoped System.ReadOnlySpan<byte> message)
	{
		System.Console.Out.Write("[Server] WARNING: ");
		System.Console.Out.WriteLine(ConsoleLogger.Str(message));
	}

	public void LogError(scoped System.ReadOnlySpan<byte> message)
	{
		System.Console.Error.Write("[Server] ERROR: ");
		System.Console.Error.WriteLine(ConsoleLogger.Str(message));
	}

	public void LogException(System.Exception ex)
	{
		System.Console.Error.Write("[Server] EXCEPTION: ");
		System.Console.Error.WriteLine(ex);
	}

	private static string Str(scoped System.ReadOnlySpan<byte> message) =>
		System.Text.Encoding.UTF8.GetString(message);
}

// Usage:

var config = new Config(IPv4.Local, 6969)
{
	Logger = new ConsoleLogger(),
};
var server = new Server(config, RequestHandler);
```
