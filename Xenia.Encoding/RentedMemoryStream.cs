using System.Buffers;
using System.IO;

namespace Byrone.Xenia
{
	/// <summary>
	/// A <see cref="MemoryStream"/> that uses a rented buffer from the <see cref="ArrayPool{T}.Shared"/> <see cref="ArrayPool{T}"/>.
	/// </summary>
	public sealed class RentedMemoryStream : MemoryStream
	{
		public RentedMemoryStream(int size) : base(ArrayPool<byte>.Shared.Rent(size), 0, size, true, true)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ArrayPool<byte>.Shared.Return(this.GetBuffer());
			}

			base.Dispose(disposing);
		}
	}
}
