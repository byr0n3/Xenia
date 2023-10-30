using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	public enum ResponseEncoding
	{
		None = 0,
		GZip,
		Deflate,
		Brotli,
	}
}
