using System.Diagnostics.CodeAnalysis;
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

		public CompressionMethod SupportedCompression { get; init; }

		public string[]? StaticFiles { get; init; }

		[SetsRequiredMembers]
		public ServerOptions(string ip, int port, CompressionMethod compression) : this(ip, port, null, compression)
		{
		}

		[SetsRequiredMembers]
		public ServerOptions(string ip,
							 int port,
							 IServerLogger? logger = null,
							 CompressionMethod compression = CompressionMethod.All)
		{
			this.IpAddress = ip;
			this.Port = port;
			this.Logger = logger;
			this.SupportedCompression = compression;
		}
	}
}
