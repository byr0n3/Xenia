using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Byrone.Xenia
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct StatusCode : System.IEquatable<StatusCode>
	{
		public required int Code { get; init; }

		public SpanPointer<byte> Message { get; init; }

		[SetsRequiredMembers]
		public StatusCode(int code, System.ReadOnlySpan<byte> message)
		{
			this.Code = code;
			this.Message = message;
		}

		public bool Equals(StatusCode other) =>
			this.Code == other.Code && this.Message == other.Message;

		public override bool Equals(object? obj) =>
			obj is StatusCode other && this.Equals(other);

		public override int GetHashCode() =>
			this.Code;

		public static bool operator ==(StatusCode left, StatusCode right) =>
			left.Equals(right);

		public static bool operator !=(StatusCode left, StatusCode right) =>
			!left.Equals(right);
	}
}
