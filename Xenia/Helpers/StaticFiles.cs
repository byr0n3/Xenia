using System.IO;
using Byrone.Xenia.Data;
using Byrone.Xenia.Internal;

namespace Byrone.Xenia.Helpers
{
	internal static class StaticFiles
	{
		public static FileInfo? GetStaticFileInfo(string[] directories, System.ReadOnlySpan<byte> path)
		{
			// remove leading slash
			path = path.Slice(1);

			var idx = System.MemoryExtensions.LastIndexOf(path, Characters.ForwardSlash);
			var dir = new BytePointer(idx == -1 ? default : path.Slice(0, idx)).ToString();
			var fileName = new BytePointer(idx == -1 ? path : path.Slice(idx + 1)).ToString();

			if (fileName is null)
			{
				return null;
			}

			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var directory in directories)
			{
				// @todo Optimize
				var fullPath = dir is null ? Path.Combine(directory, fileName) : Path.Combine(directory, dir, fileName);

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
