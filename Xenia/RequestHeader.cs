using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct RequestHeader
	{
		public SpanPointer<byte> Key { get; }

		public SpanPointer<byte> Value { get; }

		public RequestHeader(System.ReadOnlySpan<byte> key, System.ReadOnlySpan<byte> value)
		{
			this.Key = key;
			this.Value = value;
		}
	}
}
