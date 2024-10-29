using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Byrone.Xenia.Internal.Extensions;

namespace Byrone.Xenia.Utilities
{
	[InterpolatedStringHandler]
	[StructLayout(LayoutKind.Sequential)]
	public ref struct StringBuilder
	{
		private readonly System.Span<byte> buffer;
		private int position;

		public readonly System.ReadOnlySpan<byte> Result =>
			this.buffer.SliceUnsafe(0, this.position);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StringBuilder(System.Span<byte> buffer) =>
			this.buffer = buffer;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StringBuilder(int _, int __, System.Span<byte> buffer) : this(buffer)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly System.Span<byte> Slice(int length = 0)
		{
			if (length <= 0)
			{
				length = this.buffer.Length - this.position;
			}

			return this.buffer.SliceUnsafe(this.position, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Move(int length) =>
			this.position += length;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public System.ReadOnlySpan<byte> GetAndReset()
		{
			var result = this.Result;

			this.Reset();

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			this.position = 0;
		}

		public void AppendLiteral(string @string)
		{
			var status = Utf8.FromUtf16(@string, this.Slice(), out _, out var written);

			Debug.Assert(status == OperationStatus.Done);

			this.Move(written);
		}

		public void AppendFormatted(scoped System.ReadOnlySpan<byte> @string)
		{
			var length = @string.Length;

			if (length == 0)
			{
				return;
			}

			var result = @string.TryCopyTo(this.Slice());

			Debug.Assert(result);

			this.Move(@string.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(byte value)
		{
			Debug.Assert(this.position <= this.buffer.Length - 1);

			this.buffer[this.position++] = value;
		}

		public void AppendFormatted(char value)
		{
			if (char.IsAscii(value))
			{
				this.AppendFormatted((byte)value);
			}
			else
			{
				var result = Rune.TryCreate(value, out var rune);

				Debug.Assert(result);

				result = rune.TryEncodeToUtf8(this.Slice(), out var written);

				Debug.Assert(result);

				this.Move(written);
			}
		}

		public void AppendFormatted<T>(T value, scoped System.ReadOnlySpan<char> format = default)
			where T : unmanaged, System.IUtf8SpanFormattable
		{
			var result = value.TryFormat(this.Slice(), out var written, format, CultureInfo.InvariantCulture);

			Debug.Assert(result);

			this.Move(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.ReadOnlySpan<byte> Format(System.Span<byte> buffer,
													   [InterpolatedStringHandlerArgument(nameof(buffer))]
													   scoped StringBuilder builder) =>
			buffer.SliceUnsafe(0, builder.position);
	}
}
