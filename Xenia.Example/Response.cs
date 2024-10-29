using System.Net.Sockets;

namespace Byrone.Xenia.Example
{
	internal readonly struct Response : IResponse
	{
		public void Send(Socket client, in Request _)
		{
			var response = """
						   HTTP/1.1 200 OK
						   Content-Type: text/html

						   <html><body><h1>Hello world!</h1></body></html>
						   """u8;

			client.Send(response);
		}
	}
}
