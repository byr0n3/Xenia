using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Byrone.Xenia.Helpers
{
	public partial struct ResponseBuilder
	{
		public bool TryAppendFormattable<T>(T value,
											scoped System.ReadOnlySpan<char> format = default,
											System.IFormatProvider? provider = default)
			where T : unmanaged, System.IUtf8SpanFormattable
		{
			var slice = this.Take(0);

			if (!value.TryFormat(slice, out var written, format, provider ?? CultureInfo.InvariantCulture))
			{
				return false;
			}

			this.Move(written);

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append<T>(T value) where T : unmanaged, INumber<T>, System.IUtf8SpanFormattable =>
			this.TryAppendFormattable(value, default, NumberFormatInfo.InvariantInfo);

		public void Append(System.DateOnly value)
		{
			this.Append(value.Year);
			this.Append('-');
			this.AppendPad(value.Month);
			this.Append('-');
			this.AppendPad(value.Day);
		}

		public void Append(System.DateTime value)
		{
			var universalTime = value.ToUniversalTime();

			this.Append(System.DateOnly.FromDateTime(value));

			this.Append('T');

			this.AppendPad(universalTime.Hour);
			this.Append(':');
			this.AppendPad(universalTime.Minute);
			this.Append(':');
			this.AppendPad(universalTime.Second);

			this.Append('Z');
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AppendPad(int value)
		{
			if (value < 10U)
			{
				this.Append('0');
			}

			this.Append(value);
		}
	}
}
