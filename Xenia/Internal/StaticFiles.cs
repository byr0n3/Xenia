using System.IO;
using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;

namespace Byrone.Xenia.Internal
{
	internal static class StaticFiles
	{
		// @todo Optimize
		public static FileInfo? GetStaticFileInfo(StaticFileDirectory[]? directories, System.ReadOnlySpan<byte> path)
		{
			if (directories is null)
			{
				return null;
			}

			// remove leading slash
			path = path.Slice(1);

			var idx = System.MemoryExtensions.LastIndexOf(path, Characters.ForwardSlash);
			var dir = new BytePointer(idx == -1 ? default : path.Slice(0, idx)).ToString() ?? string.Empty;
			var fileName = new BytePointer(idx == -1 ? path : path.Slice(idx + 1)).ToString();

			if (fileName is null)
			{
				return null;
			}

			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var directory in directories)
			{
				if (directory.RequireBase && !CheckBasePrefix(directory.Path, path))
				{
					continue;
				}

				var fullPath = directory.RequireBase ? Path.Combine(dir, fileName) : Path.Combine(directory.Path, dir, fileName);

				var info = new FileInfo(fullPath);

				if (info.Exists)
				{
					return info;
				}
			}

			return null;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static bool CheckBasePrefix(string basePath, BytePointer path) =>
				path.ToString()?.StartsWith(basePath, System.StringComparison.Ordinal) == true;
		}
	}
}
