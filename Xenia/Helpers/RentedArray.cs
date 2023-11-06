using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Byrone.Xenia.Internal;
using JetBrains.Annotations;

namespace Byrone.Xenia.Helpers
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct RentedArray<T> : IEnumerable<RentedArray<T>.Enumerator, T>, System.IDisposable
		where T : unmanaged
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

		public RentedArray(T[] data, ArrayPool<T> pool, int count = 0)
		{
			this.pool = pool;

			this.Data = data;
			this.Size = count == 0 ? data.Length : count;
		}

		public RentedArray(int baseSize, ArrayPool<T> pool, bool useArrayLength = false)
		{
			this.pool = pool;

			this.Data = this.pool.Rent(baseSize);
			this.Size = useArrayLength ? this.Data.Length : baseSize;
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

		public void Dispose()
		{
			if (this.pool is not null && this.Data is not null)
			{
				this.pool.Return(this.Data);
			}
		}

		public Enumerator GetEnumerator() =>
			new(this.Data, this.Size);

		public struct Enumerator : IEnumerator<T>
		{
			public required T[] Data { get; init; }
			public required int Size { get; init; }

			private int current;

			[SetsRequiredMembers]
			public Enumerator(T[] data, int size)
			{
				this.Data = data;
				this.Size = size;
				this.current = -1;
			}

			public readonly T Current =>
				this.Data[this.current];

			public bool MoveNext()
			{
				if (this.current == this.Size - 1)
				{
					return false;
				}

				this.current++;
				return true;
			}
		}
	}
}
