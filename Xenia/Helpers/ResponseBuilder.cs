using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace Byrone.Xenia.Helpers
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public partial struct ResponseBuilder : System.IDisposable
	{
		private RentedArray<byte> buffer;
		private int position;

		public readonly System.Span<byte> Span =>
			this.buffer.AsSpan(0, this.position);

		public readonly int Capacity =>
			this.buffer.Size;

		public ResponseBuilder() : this(1024)
		{
		}

		public ResponseBuilder(int size)
		{
			this.buffer = new RentedArray<byte>(size);
			this.position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly System.Span<byte> Take(int size) =>
			this.buffer.AsSpan(this.position, size);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Move(int length) =>
			this.position += length;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(byte value)
		{
			this.EnsureAvailable(sizeof(byte));

			this.buffer[this.position++] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(char value) =>
			this.Append((byte)value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(System.ReadOnlySpan<byte> value)
		{
			this.EnsureAvailable(value.Length);

			var dst = this.Take(value.Length);

			value.CopyTo(dst);

			this.Move(value.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(System.ReadOnlySpan<char> value)
		{
			var size = Encoding.UTF8.GetByteCount(value);

			this.EnsureAvailable(size);

			var dst = this.Take(size);

			var written = Encoding.UTF8.GetBytes(value, dst);

			this.Move(written);
		}

		private void Resize(int add)
		{
			var newBuffer = new RentedArray<byte>(this.Capacity + add, true);

			this.buffer.AsSpan(0, this.position).CopyTo(newBuffer.AsSpan());

			this.buffer.Dispose();

			this.buffer = newBuffer;
		}

		private void EnsureAvailable(int size)
		{
			if ((this.position + size) >= this.Capacity)
			{
				this.Resize(size);
			}
		}

		public readonly void Dispose() =>
			this.buffer.Dispose();
	}
}
