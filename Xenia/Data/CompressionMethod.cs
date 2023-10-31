using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[System.Flags]
	public enum CompressionMethod
	{
		None = 0,
		GZip = 1 << 0,
		Deflate = 1 << 1,
		Brotli = 1 << 2,

		All = CompressionMethod.GZip | CompressionMethod.Deflate | CompressionMethod.Brotli,
	}
}
