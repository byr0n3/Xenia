using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Internal.Extensions;

namespace Byrone.Xenia.Utilities
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly unsafe struct Unmanaged
	{
		private readonly byte* ptr;
		private readonly int length;

		/// <summary>
		/// Convert the assigned <see cref="ptr"/> to a managed <see cref="System.ReadOnlySpan{T}"/> using the assigned <see cref="length"/>.
		/// </summary>
		public System.ReadOnlySpan<byte> Managed =>
			new(this.ptr, this.length);

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
	}
}
