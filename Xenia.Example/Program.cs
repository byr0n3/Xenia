using System.Threading;
using Byrone.Xenia;
using Byrone.Xenia.Example;
using Byrone.Xenia.Utilities;

var config = new Config(IPv4.Local, 6969)
{
	Logger = new ConsoleLogger(),
};

var server = new Server(config, RequestHandler);

var thread = new Thread(server.Run)
{
	IsBackground = true,
	Name = "Xenia Server Thread",
};
thread.Start();

while (true)
{
	if (System.Console.ReadKey(true).Key == System.ConsoleKey.C)
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

static IResponse RequestHandler(in Request request)
{
	var routes = new RouteCollection([
		"/"u8,
		"/blog"u8,
		"/blog/{post}"u8,
	]);

	if (routes.TryFind(request.Path, out var pattern))
	{
		var @params = new RouteParameters(pattern, request.Path);
		var post = @params.Get("post"u8);    // Will return: my-first-post
		var id = @params.Get<int>("post"u8); // You can also parse this value to value type
	}

	return new Response();
}
