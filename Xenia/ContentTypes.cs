using Byrone.Xenia.Data;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	public static class ContentTypes
	{
		public static readonly BytePointer Html = "text/html"u8;

		public static readonly BytePointer Json = "application/json"u8;

		public static readonly BytePointer MultipartFormData = "multipart/form-data"u8;
	}
}
