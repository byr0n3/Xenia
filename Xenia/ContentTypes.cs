using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	public static class ContentTypes
	{
		public static readonly SpanPointer<byte> Html = "text/html"u8;

		public static readonly SpanPointer<byte> Json = "application/json"u8;

		public static readonly SpanPointer<byte> MultipartFormData = "multipart/form-data"u8;
	}
}
