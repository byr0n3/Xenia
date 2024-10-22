using System.Threading;
using Byrone.Xenia;
using Byrone.Xenia.Utilities;
using Xenia.Example;

var cts = new CancellationTokenSource();

var config = new Config(IPv4.Local, 5000)
{
	Logger = new ConsoleLogger(),
};

var server = new Server(config);

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
