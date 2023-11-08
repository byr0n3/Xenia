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

		public LogLevel LogLevel { get; init; }

		public CompressionMethod SupportedCompression { get; init; }

		public StaticFileDirectory[]? StaticFiles { get; init; }

		[SetsRequiredMembers]
		public ServerOptions(string ip,
							 int port,
							 IServerLogger? logger = null,
							 LogLevel logLevel = LogLevel.All,
							 CompressionMethod compression = CompressionMethod.All)
		{
			this.IpAddress = ip;
			this.Port = port;
			this.Logger = logger;
			this.LogLevel = logLevel;
			this.SupportedCompression = compression;
		}
	}
}
