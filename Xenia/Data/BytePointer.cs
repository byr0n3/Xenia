using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	/// <summary>
	/// A small struct to replace a byte <see cref="System.Span"/>.
	/// This struct can be used where a <see cref="System.Span"/> isn't allowed, with the nice addition of being able to convert this directly to a string, with minimal allocation.
	/// </summary>
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public readonly unsafe struct BytePointer : System.IEquatable<BytePointer>
	{
		private byte* Ptr { get; }

		private int Length { get; }

		public bool Empty =>
			this.Ptr == null || this.Length == 0;

		public BytePointer()
		{
			this.Ptr = null;
			this.Length = 0;
		}

		public BytePointer(byte* ptr, int length)
		{
			this.Ptr = ptr;
			this.Length = length;
		}

		public BytePointer(System.ReadOnlySpan<byte> value)
		{
			this.Ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(value));
			this.Length = value.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public System.ReadOnlySpan<byte> AsSpan(int length = 0) =>
			this.AsWriteableSpan(length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public System.Span<byte> AsWriteableSpan(int length = 0)
		{
			if (length <= 0)
			{
				length = this.Length;
			}

			return new System.Span<byte>(this.Ptr, length);
		}

		public bool Equals(BytePointer other) =>
			System.MemoryExtensions.SequenceEqual(this.AsSpan(), other);

		public override bool Equals(object? @object) =>
			@object is BytePointer other && this.Equals(other);

		public string ToString(int length) =>
			new((sbyte*)this.Ptr, 0, System.Math.Clamp(length, 1, this.Length));

		public override string? ToString() =>
			(!this.Empty) ? this.ToString(this.Length) : null;

		public override int GetHashCode()
		{
			var hashCode = new System.HashCode();

			hashCode.AddBytes(this.AsSpan());

			return hashCode.ToHashCode();
		}

		public static bool operator ==(BytePointer left, BytePointer right) =>
			left.Equals(right);

		public static bool operator !=(BytePointer left, BytePointer right) =>
			!left.Equals(right);

		public static implicit operator BytePointer(System.ReadOnlySpan<byte> value) =>
			new(value);

		public static implicit operator System.ReadOnlySpan<byte>(BytePointer value) =>
			value.AsSpan();

		public static implicit operator BytePointer(System.Span<byte> value) =>
			new(value);

		public static implicit operator System.Span<byte>(BytePointer value) =>
			value.AsWriteableSpan();
	}
}
