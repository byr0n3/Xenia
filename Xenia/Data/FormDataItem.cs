using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
#if DEBUG
	[DebuggerTypeProxy(typeof(FormDataItem.DebugView))]
#endif
	public readonly struct FormDataItem : System.IEquatable<FormDataItem>
	{
		public required BytePointer Name { get; init; }

		public required BytePointer Content { get; init; }

		public BytePointer ContentType { get; init; }

		public BytePointer FileName { get; init; }

		public bool Equals(FormDataItem other) =>
			this.Name.Equals(other.Name) &&
			this.ContentType.Equals(other.ContentType) &&
			this.Content.Equals(other.Content) &&
			this.FileName.Equals(other.FileName);

		public override bool Equals(object? @object) =>
			@object is FormDataItem other && this.Equals(other);

		public override int GetHashCode() =>
			System.HashCode.Combine(this.Name, this.ContentType, this.Content, this.FileName);

		public static bool operator ==(in FormDataItem left, in FormDataItem right) =>
			left.Equals(right);

		public static bool operator !=(in FormDataItem left, in FormDataItem right) =>
			!left.Equals(right);

#if DEBUG
		private sealed class DebugView
		{
			public required string? Name { get; init; }

			public required string? Content { get; init; }

			public required string? ContentType { get; init; }

			public required string? FileName { get; init; }

			public DebugView(FormDataItem formData)
			{
				this.Name = formData.Name.ToString();
				this.Content = formData.Content.ToString();
				this.ContentType = formData.ContentType.ToString();
				this.FileName = formData.FileName.ToString();
			}
		}
#endif
	}
}
