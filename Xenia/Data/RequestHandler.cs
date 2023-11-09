using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public readonly unsafe struct RequestHandler : System.IEquatable<RequestHandler>
	{
		public delegate void RequestHandlerCallback(in Request request, ref ResponseBuilder response);

		public required HttpMethod Method { get; init; }

		public required BytePointer Path { get; init; }

		private readonly delegate*<in Request, ref ResponseBuilder, void> handlerPtr;

		public RequestHandlerCallback Handler =>
			RequestHandler.Callback(this.handlerPtr!);

		[SetsRequiredMembers]
		public RequestHandler(System.ReadOnlySpan<byte> path, RequestHandlerCallback handler) : this(HttpMethod.Get, path, handler)
		{
		}

		[SetsRequiredMembers]
		public RequestHandler(HttpMethod method, System.ReadOnlySpan<byte> path, RequestHandlerCallback handler)
		{
			this.Method = method;
			this.Path = path;

			this.handlerPtr = RequestHandler.Ptr(handler);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static delegate*<in Request, ref ResponseBuilder, void> Ptr(RequestHandlerCallback handler) =>
			(delegate*<in Request, ref ResponseBuilder, void>)Marshal.GetFunctionPointerForDelegate(handler);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static RequestHandlerCallback Callback(delegate*<in Request, ref ResponseBuilder, void> ptr) =>
			Marshal.GetDelegateForFunctionPointer<RequestHandlerCallback>((System.IntPtr)ptr);
	}
}
