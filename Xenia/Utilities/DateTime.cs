using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia.Utilities
{
	/// <summary>
	/// Wrapper/replacement for <see cref="System.DateTime"/> as it doesn't implement <see cref="System.IUtf8SpanParsable{T}"/>.
	/// </summary>
	[PublicAPI]
	[StructLayout(LayoutKind.Explicit)]
	public readonly struct DateTime : System.IEquatable<DateTime>,
									  System.IComparable<DateTime>,
									  System.IUtf8SpanParsable<DateTime>
	{
		[FieldOffset(0)] public readonly System.DateTime Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DateTime(System.DateTime value) =>
			this.Value = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DateTime(int year, int month, int day) =>
			this.Value = new System.DateTime(year, month, day);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator DateTime(System.DateTime value) =>
			Unsafe.BitCast<System.DateTime, DateTime>(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator System.DateTime(DateTime value) =>
			Unsafe.BitCast<DateTime, System.DateTime>(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(DateTime other) =>
			this.Value.Equals(other.Value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj) =>
			obj is DateTime other && this.Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() =>
			this.Value.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(DateTime left, DateTime right) =>
			left.Equals(right);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(DateTime left, DateTime right) =>
			!left.Equals(right);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(DateTime other) =>
			this.Value.CompareTo(other.Value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(DateTime left, DateTime right) =>
			left.CompareTo(right) < 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(DateTime left, DateTime right) =>
			left.CompareTo(right) <= 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(DateTime left, DateTime right) =>
			left.CompareTo(right) > 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(DateTime left, DateTime right) =>
			left.CompareTo(right) >= 0;

		public static DateTime Parse(System.ReadOnlySpan<byte> utf8Text, System.IFormatProvider? _)
		{
			var parsed = DateTime.TryParse(utf8Text, null, out var result);

			Debug.Assert(parsed);

			return result;
		}

		// @todo Support JS ISO format
		public static bool TryParse(System.ReadOnlySpan<byte> utf8Text,
									System.IFormatProvider? _,
									out DateTime result)
		{
			if (!Utf8Parser.TryParse(utf8Text, out System.DateTime dateTime, out var __, 'O'))
			{
				result = default;
				return false;
			}

			result = dateTime;
			return true;
		}
	}
}
