using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Byrone.Xenia.Internal
{
	internal readonly struct RentedArray<T> : System.IDisposable
	{
		private readonly int size;
		private readonly T[]? array;
		private readonly ArrayPool<T>? pool;

		/// <summary>
		/// Checks if <see name="array"/> and <see cref="pool"/> are not null.
		/// </summary>
		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[MemberNotNullWhen(true, nameof(this.array), nameof(this.pool))]
			get => (this.array is not null) && (this.pool is not null);
		}

		/// <summary>
		/// The entire rented <see cref="array"/>.
		/// </summary>
		public System.Span<T> Array
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				Debug.Assert(this.IsValid);

				return MemoryMarshal.CreateSpan(ref this.array[0], this.array.Length);
			}
		}

		/// <summary>
		/// A slice of the rented <see cref="array"/>, starting from the first element with a length of <see cref="size"/>.
		/// </summary>
		public System.Span<T> Span
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				Debug.Assert(this.IsValid);

				return MemoryMarshal.CreateSpan(ref this.array[0], this.size);
			}
		}

		public ref T this[int index]
		{
			get
			{
				Debug.Assert(this.IsValid);

				return ref this.array[index];
			}
		}

		[System.Obsolete("Use constructor with arguments instead.", true)]
		public RentedArray()
		{
		}

		public RentedArray(int size) : this(ArrayPool<T>.Shared, size)
		{
		}

		/// <summary>
		/// Rent a array with a minimum length of <paramref name="size"/> from <paramref name="pool"/>.
		/// </summary>
		/// <param name="pool">The <see cref="ArrayPool{T}"/> to rent from.</param>
		/// <param name="size">The minimum length of the rented array.</param>
		public RentedArray(ArrayPool<T> pool, int size)
		{
			this.pool = pool;
			this.size = size;

			this.array = pool.Rent(size);
		}

		public void Dispose()
		{
			if (this.IsValid)
			{
				this.pool.Return(this.array);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator System.Span<T>(RentedArray<T> array) =>
			array.Span;
	}
}
