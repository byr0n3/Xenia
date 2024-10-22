using System.Runtime.InteropServices;

namespace Byrone.Xenia
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly ref struct StatusCode
	{
		public readonly ushort Code;

		public readonly System.ReadOnlySpan<byte> Message;

		public StatusCode(ushort code, System.ReadOnlySpan<byte> message)
		{
			this.Code = code;
			this.Message = message;
		}
	}
}
