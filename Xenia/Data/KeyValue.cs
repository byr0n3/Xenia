using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
#if DEBUG
	[DebuggerTypeProxy(typeof(KeyValue.DebugView))]
#endif
	public readonly struct KeyValue : System.IEquatable<KeyValue>
	{
		public required BytePointer Key { get; init; }

		public required BytePointer Value { get; init; }

		[SetsRequiredMembers]
		public KeyValue(System.ReadOnlySpan<byte> key, System.ReadOnlySpan<byte> value)
		{
			this.Key = key;
			this.Value = value;
		}

		public bool Equals(KeyValue other) =>
			this.Key.Equals(other.Key) && this.Value.Equals(other.Value);

		public override bool Equals(object? @object) =>
			@object is KeyValue other && this.Equals(other);

		public override int GetHashCode() =>
			System.HashCode.Combine(this.Key, this.Value);

		public static bool operator ==(KeyValue left, KeyValue right) =>
			left.Equals(right);

		public static bool operator !=(KeyValue left, KeyValue right) =>
			!left.Equals(right);

#if DEBUG
		private sealed class DebugView
		{
			public required string? Key { get; init; }

			public required string? Value { get; init; }

			public DebugView(KeyValue header)
			{
				this.Key = header.Key.ToString();
				this.Value = header.Value.ToString();
			}
		}
#endif
	}
}
