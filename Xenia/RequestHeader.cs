using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Byrone.Xenia
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct RequestHeader
	{
		private unsafe byte* KeyStart { get; init; }

		private int KeyLength { get; init; }

		private unsafe byte* ValueStart { get; init; }

		private int ValueLength { get; init; }

		public unsafe System.ReadOnlySpan<byte> Key =>
			new(this.KeyStart, this.KeyLength);

		public unsafe System.ReadOnlySpan<byte> Value =>
			new(this.ValueStart, this.ValueLength);

		public unsafe RequestHeader(System.ReadOnlySpan<byte> key, System.ReadOnlySpan<byte> value)
		{
			this.KeyStart = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(key));
			this.KeyLength = key.Length;

			this.ValueStart = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(value));
			this.ValueLength = value.Length;
		}
	}
}
