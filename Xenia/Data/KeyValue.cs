using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Byrone.Xenia.Helpers;
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
		public required SpanPointer<byte> Key { get; init; }

		public required SpanPointer<byte> Value { get; init; }

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
			private static readonly System.Text.Encoding encoding = System.Text.Encoding.Latin1;

			public required string Key { get; init; }

			public required string Value { get; init; }

			public DebugView(KeyValue header)
			{
				this.Key = DebugView.encoding.GetString(header.Key);
				this.Value = DebugView.encoding.GetString(header.Value);
			}
		}
#endif
	}
}
