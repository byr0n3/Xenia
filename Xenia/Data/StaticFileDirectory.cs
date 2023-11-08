using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct StaticFileDirectory : System.IEquatable<StaticFileDirectory>
	{
		/// <summary>
		/// Path of the file directory on the file system.
		/// </summary>
		public required string Path { get; init; }

		/// <summary>
		/// Is the file directory path required in the request URL?
		/// </summary>
		public bool RequireBase { get; init; }

		[SetsRequiredMembers]
		public StaticFileDirectory(string path, bool requireBase = false)
		{
			this.Path = path;
			this.RequireBase = requireBase;
		}

		public bool Equals(StaticFileDirectory other) =>
			string.Equals(this.Path, other.Path, System.StringComparison.Ordinal) &&
			this.RequireBase == other.RequireBase;

		public override bool Equals(object? @object) =>
			@object is StaticFileDirectory other && this.Equals(other);

		public override int GetHashCode() =>
			System.HashCode.Combine(this.Path, this.RequireBase);

		public static bool operator ==(StaticFileDirectory left, StaticFileDirectory right) =>
			left.Equals(right);

		public static bool operator !=(StaticFileDirectory left, StaticFileDirectory right) =>
			!left.Equals(right);
	}
}
