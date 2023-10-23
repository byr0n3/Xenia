using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	public static class Headers
	{
		public static readonly SpanPointer<byte> AcceptContent = "Accept-Content"u8;

		public static readonly SpanPointer<byte> AcceptEncoding = "Accept-Encoding"u8;

		public static readonly SpanPointer<byte> ContentType = "Content-Type"u8;
	}
}
