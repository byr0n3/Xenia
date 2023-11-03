using Byrone.Xenia.Data;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	public static class Headers
	{
		public static readonly BytePointer AcceptContent = "Accept-Content"u8;

		public static readonly BytePointer AcceptEncoding = "Accept-Encoding"u8;

		public static readonly BytePointer ContentType = "Content-Type"u8;

		public static readonly BytePointer ContentLength = "Content-Length"u8;

		public static readonly BytePointer ContentEncoding = "Content-Encoding"u8;

		public static readonly BytePointer Date = "Date"u8;

		public static readonly BytePointer Server = "Server"u8;

		public static readonly BytePointer TransferEncoding = "Transfer-Encoding"u8;
	}
}
