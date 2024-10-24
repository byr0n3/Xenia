using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;
using Byrone.Xenia.Internal;
using JetBrains.Annotations;

namespace Byrone.Xenia.Utilities
{
	/// <summary>
	/// Represents an IP.
	/// </summary>
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public readonly struct IPv4 : System.IEquatable<IPv4>,
								  System.IUtf8SpanParsable<IPv4>,
								  System.IUtf8SpanFormattable,
								  System.ISpanFormattable
	{
		public readonly byte A, B, C, D;

		public static readonly IPv4 Any = new(0, 0, 0, 0);
		public static readonly IPv4 Local = new(127, 0, 0, 1);

		private IPv4(IPAddress value)
		{
			Debug.Assert(IsIPv4(value));

			Unsafe.As<IPv4, uint>(ref this) = PrivateAddress(value);

			return;

			[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsIPv4")]
			static extern bool IsIPv4(IPAddress @this);

			[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PrivateAddress")]
			static extern uint PrivateAddress(IPAddress @this);
		}

		private IPv4(byte a, byte b, byte c, byte d)
		{
			this.A = a;
			this.B = b;
			this.C = c;
			this.D = d;
		}

		private IPv4(scoped System.ReadOnlySpan<byte> bytes)
		{
			this.A = bytes[0];
			this.B = bytes[1];
			this.C = bytes[2];
			this.D = bytes[3];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator IPv4(IPAddress value) =>
			new(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator IPAddress(IPv4 value) =>
			new(Unsafe.BitCast<IPv4, uint>(value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(IPv4 lhs, IPv4 rhs) =>
			lhs.Equals(rhs);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(IPv4 lhs, IPv4 rhs) =>
			!lhs.Equals(rhs);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(IPv4 other) =>
			(Unsafe.BitCast<IPv4, int>(this) == Unsafe.BitCast<IPv4, int>(other));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? @object) =>
			@object is IPv4 other && this.Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() =>
			Unsafe.BitCast<IPv4, int>(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() =>
			$"{this.A}.{this.B}.{this.C}.{this.D}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToString(string? _, System.IFormatProvider? __) =>
			this.ToString();

		[System.Obsolete("Not properly implemented")]
		public bool TryFormat(System.Span<char> destination,
							  out int written,
							  System.ReadOnlySpan<char> _,
							  System.IFormatProvider? __)
		{
			const int maxLength = 3 + 1 + 3 + 1 + 3 + 1 + 3;

			written = default;

			if (destination.Length < maxLength)
			{
				return false;
			}

			written += Append(ref destination, this.A, true);
			written += Append(ref destination, this.B, true);
			written += Append(ref destination, this.C, true);
			written += Append(ref destination, this.D, false);

			return true;

			static int Append(ref System.Span<char> destination, byte value, bool separator)
			{
				value.TryFormat(destination, out var written);

				if (separator)
				{
					destination[written++] = '.';
				}

				destination = destination.Slice(written);

				return written;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryFormat(System.Span<byte> destination,
							  out int written,
							  System.ReadOnlySpan<char> _,
							  System.IFormatProvider? provider) =>
			Utf8.TryWrite(destination, provider, $"{this.A}.{this.B}.{this.C}.{this.D}", out written);

		[SuppressMessage("Usage", "MA0011")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(System.ReadOnlySpan<byte> span, System.IFormatProvider? _, out IPv4 result) =>
			IPv4.TryParse(span, out result);

		public static bool TryParse(scoped System.ReadOnlySpan<byte> span, out IPv4 result)
		{
			const int bytes = 4;

			System.Span<byte> temp = stackalloc byte[bytes];
			var written = 0;

			foreach (var part in new SplitEnumerator(span, Characters.Dot))
			{
				if (!Utf8Parser.TryParse(part, out byte @byte, out _))
				{
					result = default;
					return false;
				}

				temp[written++] = @byte;
			}

			result = written == bytes ? new IPv4(temp) : default;
			return written == bytes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPv4 Parse(scoped System.ReadOnlySpan<byte> span) =>
			IPv4.Parse(span, null);

		public static IPv4 Parse(scoped System.ReadOnlySpan<byte> span, System.IFormatProvider? _)
		{
			const int bytes = 4;

			System.Span<byte> temp = stackalloc byte[bytes];
			var written = 0;

			foreach (var part in new SplitEnumerator(span, Characters.Dot))
			{
				var parsed = Utf8Parser.TryParse(part, out byte @byte, out var __);

				Debug.Assert(parsed);

				temp[written++] = @byte;
			}

			return new IPv4(temp);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPv4 From(EndPoint? endPoint)
		{
			if (endPoint is null)
			{
				return default;
			}

			Debug.Assert(endPoint is IPEndPoint);

			return ((IPEndPoint)endPoint).Address;
		}
	}
}
