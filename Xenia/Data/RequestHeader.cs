using System.Runtime.InteropServices;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct RequestHeader : System.IEquatable<RequestHeader>
	{
		public SpanPointer<byte> Key { get; }

		public SpanPointer<byte> Value { get; }

		public RequestHeader(System.ReadOnlySpan<byte> key, System.ReadOnlySpan<byte> value)
		{
			this.Key = key;
			this.Value = value;
		}

		public bool Equals(RequestHeader other) =>
			this.Key.Equals(other.Key) && this.Value.Equals(other.Value);

		public override bool Equals(object? @object) =>
			@object is RequestHeader other && this.Equals(other);

		public override int GetHashCode() =>
			System.HashCode.Combine(this.Key, this.Value);

		public static bool operator ==(RequestHeader left, RequestHeader right) =>
			left.Equals(right);

		public static bool operator !=(RequestHeader left, RequestHeader right) =>
			!left.Equals(right);
	}
}