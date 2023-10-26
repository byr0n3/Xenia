using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Byrone.Xenia.Helpers
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly unsafe struct SpanPointer<T> : System.IEquatable<SpanPointer<T>> where T : unmanaged
	{
		private T* Start { get; }

		private int Length { get; }

		public System.ReadOnlySpan<T> AsSpan =>
			new(this.Start, this.Length);

		public SpanPointer(System.ReadOnlySpan<T> value)
		{
			this.Start = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(value));
			this.Length = value.Length;
		}

		public static implicit operator SpanPointer<T>(System.ReadOnlySpan<T> value) =>
			new(value);

		public static implicit operator System.ReadOnlySpan<T>(SpanPointer<T> value) =>
			value.AsSpan;

		public bool Equals(SpanPointer<T> other) =>
			System.MemoryExtensions.SequenceEqual(this.AsSpan, other);

		public override bool Equals(object? @object) =>
			@object is SpanPointer<T> other && this.Equals(other);

		// @todo ???
		public override int GetHashCode() =>
			System.HashCode.Combine(unchecked((int)(long)this.Start), this.Length);

		public static bool operator ==(SpanPointer<T> left, SpanPointer<T> right) =>
			left.Equals(right);

		public static bool operator !=(SpanPointer<T> left, SpanPointer<T> right) =>
			!left.Equals(right);
	}
}
