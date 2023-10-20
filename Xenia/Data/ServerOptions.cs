using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct ServerOptions
	{
		public required string IpAddress { get; init; }

		public required int Port { get; init; }

		public IServerLogger? Logger { get; init; }
	}
}
