using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;
using JetBrains.Annotations;

namespace Byrone.Xenia.Utilities
{
	/// <summary>
	/// Wrapper/replacement for <see cref="System.DateOnly"/> as it doesn't implement <see cref="System.IUtf8SpanParsable{T}"/>.
	/// </summary>
	[PublicAPI]
	[StructLayout(LayoutKind.Explicit)]
	public readonly struct Date : System.IEquatable<Date>, System.IComparable<Date>, System.IUtf8SpanParsable<Date>
	{
		[FieldOffset(0)] public readonly System.DateOnly Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Date(System.DateOnly value) =>
			this.Value = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Date(int year, int month, int day) =>
			this.Value = new System.DateOnly(year, month, day);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Date(System.DateOnly value) =>
			Unsafe.BitCast<System.DateOnly, Date>(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator System.DateOnly(Date value) =>
			Unsafe.BitCast<Date, System.DateOnly>(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Date other) =>
			this.Value.Equals(other.Value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) =>
			obj is Date other && this.Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() =>
			this.Value.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Date left, Date right) =>
			left.Equals(right);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Date left, Date right) =>
			!left.Equals(right);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(Date other) =>
			this.Value.CompareTo(other.Value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Date left, Date right) =>
			left.CompareTo(right) < 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(Date left, Date right) =>
			left.CompareTo(right) <= 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Date left, Date right) =>
			left.CompareTo(right) > 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(Date left, Date right) =>
			left.CompareTo(right) >= 0;

		public static Date Parse(System.ReadOnlySpan<byte> utf8Text, System.IFormatProvider? provider)
		{
			System.Span<char> temp = stackalloc char[sizeof(char) * utf8Text.Length];

			// @todo Remove conversion
			var status = Utf8.ToUtf16(utf8Text, temp, out _, out var written);

			Debug.Assert(status == OperationStatus.Done);

			return System.DateOnly.Parse(temp.Slice(0, written), provider);
		}

		public static bool TryParse(System.ReadOnlySpan<byte> utf8Text,
									System.IFormatProvider? provider,
									out Date result)
		{
			System.Span<char> temp = stackalloc char[sizeof(char) * utf8Text.Length];

			// @todo Remove conversion
			var status = Utf8.ToUtf16(utf8Text, temp, out _, out var written);

			if (status != OperationStatus.Done)
			{
				result = default;
				return false;
			}

			if (!System.DateOnly.TryParse(temp.Slice(0, written), provider, out var date))
			{
				result = default;
				return false;
			}

			result = date;
			return true;
		}
	}
}
