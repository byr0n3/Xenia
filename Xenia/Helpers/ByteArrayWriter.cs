using System.Globalization;
using System.IO;
using System.Text;

namespace Byrone.Xenia.Helpers
{
	internal sealed class ByteArrayWriter : TextWriter
	{
		public override Encoding Encoding =>
			Encoding.UTF8;

		private readonly byte[] buffer;

		private int position;

		public System.ReadOnlySpan<byte> Data =>
			System.MemoryExtensions.AsSpan(this.buffer, 0, this.position);

		public ByteArrayWriter(byte[] buffer)
		{
			this.buffer = buffer;
			this.position = 0;
		}

		private System.Span<byte> Take(int length) =>
			System.MemoryExtensions.AsSpan(this.buffer, this.position, length);

		private void Move(int length) =>
			this.position += length;

		private void Write<T>(T value) where T : unmanaged, System.IUtf8SpanFormattable
		{
			var slice = this.Take(0);

			if (value.TryFormat(slice, out var written, default, NumberFormatInfo.InvariantInfo))
			{
				this.Move(written);
			}
		}

		public void Write(byte value) =>
			this.buffer[this.position++] = value;

		public override void Write(bool value) =>
			this.Write(value ? 1 : 0);

		#region Chars

		public override void Write(char value) =>
			this.Write((byte)value);

		public override void Write(char[]? value)
		{
			if (value is null)
			{
				return;
			}

			this.Write(System.MemoryExtensions.AsSpan(value));
		}

		public void Write(System.Span<char> value)
		{
			if (value.IsEmpty)
			{
				return;
			}

			var size = this.Encoding.GetByteCount(value);

			var dst = this.Take(size);

			var written = this.Encoding.GetBytes(value, dst);

			this.Move(written);
		}

		public override void Write(System.ReadOnlySpan<char> value)
		{
			if (value.IsEmpty)
			{
				return;
			}

			var size = this.Encoding.GetByteCount(value);

			var dst = this.Take(size);

			var written = this.Encoding.GetBytes(value, dst);

			this.Move(written);
		}

		public override void Write(string? value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}

			var size = this.Encoding.GetByteCount(value);

			var dst = this.Take(size);

			var written = this.Encoding.GetBytes(value, dst);

			this.Move(written);
		}

		public override void Write(char[] value, int start, int length) =>
			this.Write(System.MemoryExtensions.AsSpan(value, start, length));

		public override void Write(StringBuilder? value)
		{
			if (value is null)
			{
				return;
			}

			var temp = new RentedArray<char>(value.Length);

			value.CopyTo(0, temp.Data, value.Length);

			this.Write(temp.AsSpan(0, value.Length));

			temp.Dispose();
		}

		#endregion

		#region Numerics

		public override void Write(float value) =>
			this.Write(value);

		public override void Write(int value) =>
			this.Write(value);

		public override void Write(uint value) =>
			this.Write(value);

		public override void Write(ulong value) =>
			this.Write(value);

		public override void Write(long value) =>
			this.Write(value);

		public override void Write(decimal value) =>
			this.Write(value);

		public override void Write(double value) =>
			this.Write(value);

		#endregion
	}
}
