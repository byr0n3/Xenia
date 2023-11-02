using System.IO;
using System.Text;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;
using Byrone.Xenia.Internal;

namespace Byrone.Xenia.Extensions
{
	public static partial class ResponseExtensions
	{
		public static void AppendFile(this ref ResponseBuilder @this, in Request request, FileInfo info)
		{
			System.Span<char> buffer = stackalloc char[(int)info.Length];

			// can't use `OpenRead`, we expect UTF8 encoded bytes
			using (var stream = info.OpenText())
			{
				stream.Read(buffer);
			}

			var mime = MimeMapping.GetMimeMapping(info.Name);
			var size = Encoding.UTF8.GetByteCount(buffer);

			@this.AppendHeaders(in request, in StatusCodes.Status200OK, mime);

			var move = Encoding.UTF8.GetBytes(buffer, @this.Take(size));

			@this.Move(move);
		}
	}
}
