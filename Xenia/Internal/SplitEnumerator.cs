using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Internal.Extensions;

namespace Byrone.Xenia.Internal
{
	/// <summary>
	/// Enumerates over <see cref="span"/> and returns every part between <see cref="separator"/>.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal ref struct SplitEnumerator
	{
		private System.ReadOnlySpan<byte> span;
		private readonly byte separator;

		public System.ReadOnlySpan<byte> Current { get; private set; }

		public readonly int Count =>
			this.span.IsEmpty ? default : (System.MemoryExtensions.Count(this.span, this.separator) + 1);

		public SplitEnumerator(System.ReadOnlySpan<byte> span, byte separator)
		{
			this.span = span;
			this.separator = separator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SplitEnumerator GetEnumerator() =>
			this;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public System.ReadOnlySpan<byte> MoveNextAndGet()
		{
			this.MoveNext();

			return this.Current;
		}

		public bool MoveNext()
		{
			if (this.span.IsEmpty)
			{
				return false;
			}

			var index = System.MemoryExtensions.IndexOf(this.span, this.separator);

			// No more separators in the span, this is the last part
			if (index == -1)
			{
				this.Current = this.span;
				this.span = default;
			}
			else
			{
				this.Current = this.span.SliceUnsafe(0, index);
				this.span = this.span.SliceUnsafe(index + 1);
			}

			return true;
		}
	}

	/// <summary>
	/// Enumerates over <see cref="span"/> and returns every part between <see cref="separator"/>.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal ref struct SpanSplitEnumerator
	{
		private System.ReadOnlySpan<byte> span;
		private readonly System.ReadOnlySpan<byte> separator;

		public System.ReadOnlySpan<byte> Current { get; private set; }

		public readonly int Count =>
			this.span.IsEmpty ? default : (System.MemoryExtensions.Count(this.span, this.separator) + 1);

		public SpanSplitEnumerator(System.ReadOnlySpan<byte> span, System.ReadOnlySpan<byte> separator)
		{
			this.span = span;
			this.separator = separator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SpanSplitEnumerator GetEnumerator() =>
			this;

		public bool MoveNext()
		{
			if (this.span.IsEmpty)
			{
				return false;
			}

			var index = System.MemoryExtensions.IndexOf(this.span, this.separator);

			// No more separators in the span, this is the last part
			if (index == -1)
			{
				this.Current = this.span;
				this.span = default;
			}
			else
			{
				this.Current = this.span.SliceUnsafe(0, index);
				this.span = this.span.SliceUnsafe(index + this.separator.Length);
			}

			return true;
		}
	}
}
