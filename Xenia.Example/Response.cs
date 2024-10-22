using System.Net.Sockets;

namespace Byrone.Xenia.Example
{
	internal readonly struct Response : IResponse
	{
		public void Send(Socket client)
		{
			client.Send(
				"HTTP/1.1 200 OK\nContent-Type: text/html\n\n<html><body><h1>Hello world!</h1></body></html>"u8);
		}
	}
}
