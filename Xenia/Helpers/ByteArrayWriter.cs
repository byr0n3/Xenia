using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Byrone.Xenia.Helpers
{
	internal sealed class ByteArrayWriter : TextWriter
	{
		private const int initialSize = 64;

		public override Encoding Encoding =>
			Encoding.UTF8;

		private RentedArray<byte> buffer;
		private int position;

		private int Capacity =>
			this.buffer.Size;

		public System.ReadOnlySpan<byte> Data =>
			this.buffer.AsSpan(0, this.position);

		public ByteArrayWriter()
		{
			this.buffer = new RentedArray<byte>(ByteArrayWriter.initialSize, true);
			this.position = 0;
		}

		private System.Span<byte> Take(int length) =>
			this.buffer.AsSpan(this.position, length);

		private void Move(int length) =>
			this.position += length;

		private void Write<T>(T value) where T : unmanaged, System.IUtf8SpanFormattable
		{
			// @todo Performant?
			this.EnsureAvailable(Marshal.SizeOf<T>());

			var slice = this.Take(0);

			if (value.TryFormat(slice, out var written, default, NumberFormatInfo.InvariantInfo))
			{
				this.Move(written);
			}
		}

		public void Write(byte value)
		{
			this.EnsureAvailable(sizeof(byte));

			this.buffer[this.position++] = value;
		}

		public override void Write(bool value) =>
			this.Write((byte)(value ? 1 : 0));

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

			this.EnsureAvailable(size);

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

			this.EnsureAvailable(size);

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

			this.EnsureAvailable(size);

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

			this.EnsureAvailable(value.Length);

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

		private void EnsureAvailable(int size)
		{
			if ((this.position + size) >= this.Capacity)
			{
				this.Resize(size);
			}
		}

		private void Resize(int add)
		{
			var newBuffer = new RentedArray<byte>(this.Capacity + add, true);

			this.buffer.AsSpan(0, this.position).CopyTo(newBuffer.AsSpan());

			this.buffer.Dispose();

			this.buffer = newBuffer;
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			this.buffer.Dispose();
		}
	}
}
