using System.IO;
using System.Text;

namespace Byrone.Xenia.Helpers
{
	internal static class StaticFiles
	{
		public static FileInfo? GetStaticFileInfo(string[] directories, System.ReadOnlySpan<byte> path)
		{
			// remove leading slash
			path = path.Slice(1);

			var idx = System.MemoryExtensions.LastIndexOf(path, Characters.ForwardSlash);
			var dir = Encoding.UTF8.GetString(idx == -1 ? default : path.Slice(0, idx));
			var fileName = Encoding.UTF8.GetString(idx == -1 ? path : path.Slice(idx + 1));

			foreach (var directory in directories)
			{
				var fullPath = idx == -1 ? Path.Combine(directory, fileName) : Path.Combine(directory, dir, fileName);

				var info = new FileInfo(fullPath);

				if (info.Exists)
				{
					return info;
				}
			}

			return null;
		}
	}
}
