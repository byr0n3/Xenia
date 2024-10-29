using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Internal.Extensions;

namespace Byrone.Xenia.Utilities
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly unsafe struct Unmanaged : System.IEquatable<Unmanaged>
	{
		private readonly byte* ptr;
		private readonly int length;

		/// <summary>
		/// Convert the assigned <see cref="ptr"/> to a managed <see cref="System.ReadOnlySpan{T}"/> using the assigned <see cref="length"/>.
		/// </summary>
		public System.ReadOnlySpan<byte> Managed =>
			new(this.ptr, this.length);

		/// <inheritdoc cref="System.ReadOnlySpan{T}.IsEmpty"/>
		public bool IsEmpty =>
			this.Managed.IsEmpty;

		public Unmanaged(byte* ptr, int length)
		{
			this.ptr = ptr;
			this.length = length;
		}

		public Unmanaged(System.ReadOnlySpan<byte> value)
		{
			this.ptr = (byte*)Unsafe.AsPointer(ref value.GetReference());
			this.length = value.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Unmanaged(System.ReadOnlySpan<byte> value) =>
			new(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator System.ReadOnlySpan<byte>(Unmanaged value) =>
			value.Managed;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Unmanaged other) =>
			System.MemoryExtensions.SequenceEqual(this.Managed, other.Managed);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? @object) =>
			(@object is Unmanaged other) && this.Equals(other);

		[System.Obsolete("Unimplemented", true)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() =>
			default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Unmanaged left, Unmanaged right) =>
			left.Equals(right);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Unmanaged left, Unmanaged right) =>
			!left.Equals(right);
	}
}
