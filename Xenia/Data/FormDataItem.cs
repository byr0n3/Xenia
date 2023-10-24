using System.Runtime.InteropServices;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct FormDataItem : System.IEquatable<FormDataItem>
	{
		public required SpanPointer<byte> Name { get; init; }

		public required SpanPointer<byte> Content { get; init; }

		public SpanPointer<byte> ContentType { get; init; }

		public SpanPointer<byte> FileName { get; init; }

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
	}
}
