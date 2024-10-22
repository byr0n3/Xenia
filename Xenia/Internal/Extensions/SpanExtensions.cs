using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Byrone.Xenia.Internal.Extensions
{
	internal static class SpanExtensions
	{
		/// <summary>
		/// Get reference to the first element in the span. (a.k.a the span's memory location)
		/// </summary>
		/// <param name="this">The span.</param>
		/// <typeparam name="T">The type of the span's contents.</typeparam>
		/// <returns>Reference to the first element in the span.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetReference<T>(this System.Span<T> @this) where T : unmanaged =>
			ref MemoryMarshal.GetReference(@this);

		/// <summary>
		/// Get reference to the first element in the span. (a.k.a the span's memory location)
		/// </summary>
		/// <param name="this">The span.</param>
		/// <typeparam name="T">The type of the span's contents.</typeparam>
		/// <returns>Reference to the first element in the span.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetReference<T>(this System.ReadOnlySpan<T> @this) where T : unmanaged =>
			ref MemoryMarshal.GetReference(@this);

		/// <summary>Forms a slice out of the current span that begins at a specified index.</summary>
		/// <param name="this">The span.</param>
		/// <param name="start">The index at which to begin the slice.</param>
		/// <returns>A span that consists of all elements of the current span from <paramref name="start" /> to the end of the span.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.Span<T> SliceUnsafe<T>(this System.Span<T> @this, int start) where T : unmanaged =>
			@this.SliceUnsafe(start, @this.Length - start);

		/// <summary>Forms a slice out of the current span starting at a specified index for a specified length.</summary>
		/// <param name="this">The span.</param>
		/// <param name="start">The index at which to begin this slice.</param>
		/// <param name="length">The desired length for the slice.</param>
		/// <returns>A span that consists of <paramref name="length" /> elements from the current span starting at <paramref name="start" />.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.Span<T> SliceUnsafe<T>(this System.Span<T> @this, int start, int length)
			where T : unmanaged
		{
			Debug.Assert((start + length) <= @this.Length);

			return MemoryMarshal.CreateSpan(
				ref Unsafe.Add(ref @this.GetReference(), start),
				length
			);
		}

		/// <summary>Forms a slice out of the current span that begins at a specified index.</summary>
		/// <param name="this">The span.</param>
		/// <param name="start">The index at which to begin the slice.</param>
		/// <returns>A span that consists of all elements of the current span from <paramref name="start" /> to the end of the span.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.ReadOnlySpan<T> SliceUnsafe<T>(this System.ReadOnlySpan<T> @this, int start)
			where T : unmanaged =>
			@this.SliceUnsafe(start, @this.Length - start);

		/// <summary>Forms a slice out of the current span starting at a specified index for a specified length.</summary>
		/// <param name="this">The span.</param>
		/// <param name="start">The index at which to begin this slice.</param>
		/// <param name="length">The desired length for the slice.</param>
		/// <returns>A span that consists of <paramref name="length" /> elements from the current span starting at <paramref name="start" />.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.ReadOnlySpan<T> SliceUnsafe<T>(this System.ReadOnlySpan<T> @this, int start, int length)
			where T : unmanaged
		{
			Debug.Assert((start + length) <= @this.Length);

			return MemoryMarshal.CreateReadOnlySpan(
				ref Unsafe.Add(ref @this.GetReference(), start),
				length
			);
		}
	}
}
