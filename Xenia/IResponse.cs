using System.Net.Sockets;

namespace Byrone.Xenia
{
	/// <summary>
	/// Interface for implementing a response type.
	/// </summary>
	/// <remarks>An implementation <b>CAN</b> extend <see cref="System.IDisposable"/> and the server will automatically call <see cref="System.IDisposable.Dispose"/> once the <see cref="Send"/> function has been called.</remarks>
	public interface IResponse
	{
		public void Send(Socket client);
	}
}
