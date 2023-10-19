using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public partial struct ResponseBuilder : System.IDisposable
	{
		private readonly RentedArray<byte> buffer;
		private int position;

		public readonly System.Span<byte> Span =>
			this.buffer.AsSpan(0, this.position);

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

		private readonly void EnsureAvailable(int size)
		{
			if ((this.position + size) >= this.buffer.Data.Length)
			{
				// @todo Resize buffer
				throw new System.InvalidOperationException("Not enough space available");
			}
		}

		public readonly void Dispose() =>
			this.buffer.Dispose();
	}
}
