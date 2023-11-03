using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
#if DEBUG
	[DebuggerTypeProxy(typeof(StatusCode.DebugView))]
#endif
	public readonly struct StatusCode : System.IEquatable<StatusCode>
	{
		public required int Code { get; init; }

		public BytePointer Message { get; init; }

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

#if DEBUG
		private sealed class DebugView
		{
			public required int Code { get; init; }

			public required string? Message { get; init; }

			public DebugView(StatusCode statusCode)
			{
				this.Code = statusCode.Code;
				this.Message = statusCode.Message.ToString();
			}
		}
#endif
	}
}
