using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Byrone.Xenia.Utilities
{
	/// <summary>
	/// Represents an IP (<see cref="IPv4"/>) and a port.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct Address : System.IEquatable<Address>, System.IUtf8SpanFormattable, System.ISpanFormattable
	{
		public readonly IPv4 Ip;
		public readonly ushort Port;

		public Address(IPv4 ip, ushort port)
		{
			this.Ip = ip;
			this.Port = port;
		}

		public Address(IPAddress ip, ushort port)
		{
			this.Ip = ip;
			this.Port = port;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Address(IPEndPoint endPoint) =>
			new(endPoint.Address, (ushort)endPoint.Port);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator IPEndPoint(Address address) =>
			new((IPAddress)address.Ip, address.Port);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Address lhs, Address rhs) =>
			lhs.Equals(rhs);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Address lhs, Address rhs) =>
			!lhs.Equals(rhs);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Address other) =>
			this.Ip == other.Ip && this.Port == other.Port;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? @object) =>
			@object is Address other && this.Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() =>
			this.Port ^ Unsafe.BitCast<IPv4, int>(this.Ip);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() =>
			$"{nameof(this.Ip)}: {this.Ip}, {nameof(this.Port)}: {this.Port}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToString(string? _, System.IFormatProvider? __) =>
			this.ToString();

		[System.Obsolete("Not properly implemented")]
		public bool TryFormat(System.Span<char> destination,
							  out int written,
							  System.ReadOnlySpan<char> _,
							  System.IFormatProvider? __)
		{
			if (!this.Ip.TryFormat(destination, out var ipWritten, null, null))
			{
				written = default;
				return false;
			}

			written = ipWritten;
			destination[written++] = ':';

			if (!this.Port.TryFormat(destination.Slice(written),
									 out var portWritten,
									 null,
									 NumberFormatInfo.InvariantInfo))
			{
				return false;
			}

			written += portWritten;
			return true;
		}

		[System.Obsolete("Not properly implemented")]
		public bool TryFormat(System.Span<byte> destination,
							  out int written,
							  System.ReadOnlySpan<char> _,
							  System.IFormatProvider? __)
		{
			if (!this.Ip.TryFormat(destination, out var ipWritten, null, null))
			{
				written = default;
				return false;
			}

			written = ipWritten;
			destination[written++] = (byte)':';

			if (!this.Port.TryFormat(destination.Slice(written),
									 out var portWritten,
									 null,
									 NumberFormatInfo.InvariantInfo))
			{
				return false;
			}

			written += portWritten;
			return true;
		}
	}
}
