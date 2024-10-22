using System.Threading;
using Byrone.Xenia;
using Byrone.Xenia.Example;
using Byrone.Xenia.Utilities;
using Xenia.Example;

var cts = new CancellationTokenSource();

var config = new Config(IPv4.Local, 6969)
{
	Logger = new ConsoleLogger(),
};

var server = new Server(config, RequestHandler);

var thread = new Thread(() => server.Run(cts.Token))
{
	IsBackground = true,
	Name = "Xenia Server Thread",
};
thread.Start();

while (!cts.IsCancellationRequested)
{
	if (System.Console.ReadKey().Key != System.ConsoleKey.Escape)
	{
		// @todo Sleep?
		return;
	}

	cts.Cancel();
	break;
}

server.Dispose();

return;

static IResponse RequestHandler(in Request request)
{
	return new Response();
}
