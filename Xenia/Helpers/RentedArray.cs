using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia.Helpers
{
	[PublicAPI]
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

		public RentedArray(int size, bool useArrayLength) : this(size, ArrayPool<T>.Shared, useArrayLength)
		{
		}

		public RentedArray(int baseSize, ArrayPool<T> pool, bool useArrayLength = false)
		{
			this.pool = pool;

			this.Data = this.pool.Rent(baseSize);
			this.Size = useArrayLength ? this.Data.Length : baseSize;

			Debug.Assert(this.Data.Length >= this.Size);
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
