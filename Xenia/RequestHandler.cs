using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Byrone.Xenia
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct RequestHandler : System.IEquatable<RequestHandler>
	{
		public delegate void Callback(ResponseWriter writer, in Request request);

		public required HttpMethod Method { get; init; }

		public required SpanPointer<byte> Path { get; init; }

		public required Callback Handler { get; init; }

		[SetsRequiredMembers]
		public RequestHandler(System.ReadOnlySpan<byte> path, Callback handler) : this(HttpMethod.Get, path, handler)
		{
		}

		[SetsRequiredMembers]
		public RequestHandler(HttpMethod method, System.ReadOnlySpan<byte> path, Callback handler)
		{
			this.Method = method;
			this.Path = path;
			this.Handler = handler;
		}

		public bool Equals(RequestHandler other) =>
			this.Method == other.Method && this.Path == other.Path;

		public override bool Equals(object? @this) =>
			@this is RequestHandler other && this.Equals(other);

		public override int GetHashCode() =>
			System.HashCode.Combine((int)this.Method, this.Path);

		public static bool operator ==(RequestHandler left, RequestHandler right) =>
			left.Equals(right);

		public static bool operator !=(RequestHandler left, RequestHandler right) =>
			!left.Equals(right);
	}
}
