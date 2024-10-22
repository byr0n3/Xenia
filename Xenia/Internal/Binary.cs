using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Internal.Extensions;

namespace Byrone.Xenia.Internal
{
	internal static class Binary
	{
		/// <summary>
		/// Parses the given characters to a byte value.
		/// </summary>
		/// <param name="span">The characters.</param>
		/// <returns>The parsed byte.</returns>
		public static unsafe byte ParseByte(scoped System.ReadOnlySpan<byte> span)
		{
			Debug.Assert(!span.IsEmpty && span.Length <= 3);

			Unsafe.SkipInit(out uint result);

			fixed (byte* src = &span.GetReference())
			{
				NativeMemory.Copy(src, &result, sizeof(uint));
			}

			// Trick to shift off unnecessary data.
			result &= 0x0f_0f_0f;
			result *= 0x64_0a_01;
			result >>= (span.Length << 3) - 8;

			return (byte)result;
		}

		/// <summary>
		/// Attempts to parse the given characters to a byte value.
		/// </summary>
		/// <param name="span">The characters.</param>
		/// <param name="result">The parsed result.</param>
		/// <returns>true if parsing was successful, false otherwise.</returns>
		[SkipLocalsInit]
		public static unsafe bool TryParseByte(scoped System.ReadOnlySpan<byte> span, out byte result)
		{
			if (span.IsEmpty || span.Length > 3)
			{
				result = default;
				return false;
			}

			Unsafe.SkipInit(out uint value);

			fixed (byte* src = &span.GetReference())
			{
				NativeMemory.Copy(src, &value, sizeof(uint));
			}

			value ^= 0x30_30_30;              // flip 0x30, detect non-digits
			value <<= (span.Length ^ 3) << 3; // shift off trash bytes

			if ((((value + 0x76_76_76) | value) & 0x80_80_80) != 0 ||
				BinaryPrimitives.ReverseEndianness(value) > 0x020505ff)
			{
				result = default;
				return false;
			}

			// Trick to shift off unnecessary data.
			value *= 0x64_0a_01;
			value >>= 16;

			result = (byte)value;
			return true;
		}
	}
}
