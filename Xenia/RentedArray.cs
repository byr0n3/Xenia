using System.Buffers;
using System.Runtime.InteropServices;

namespace Byrone.Xenia
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct RentedArray<T> : System.IDisposable
	{
		public readonly int Size;

		public readonly T[] Data;

		private readonly ArrayPool<T> pool;

		public T this[int index]
		{
			get => this.Data[index];
			set => this.Data[index] = value;
		}

		public RentedArray(int size) : this(size, ArrayPool<T>.Shared)
		{
		}

		public RentedArray(int size, ArrayPool<T> pool)
		{
			this.pool = pool;

			this.Size = size;
			this.Data = this.pool.Rent(this.Size);
		}

		public System.Span<T> AsSpan(int start = 0, int size = 0)
		{
			start = System.Math.Clamp(start, 0, this.Size);

			if (size <= 0 || size > this.Size)
			{
				size = this.Size - start;
			}

			return System.MemoryExtensions.AsSpan(this.Data, start, size);
		}

		public void Dispose() =>
			this.pool.Return(this.Data);
	}
}
