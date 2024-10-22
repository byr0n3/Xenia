using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct Config
	{
		/// <summary>
		/// The IP address and port to bind to.
		/// </summary>
		public readonly Address Address;

		/// <summary>
		/// The maximum length of the pending connections queue.
		/// </summary>
		/// <remarks>The default is 100.</remarks>
		public int Backlog { get; init; } = 100;

		/// <summary>
		/// The size of the buffer that'll receive incoming data. (in bytes)
		/// </summary>
		/// <remarks>The default is 8kb.</remarks>
		public int ReceiveBufferSize { get; init; } = 1024 * 8; // 8 kb

		/// <summary>
		/// Logger instance to use when logging information, errors, etc.
		/// </summary>
		/// <remarks>If the logger is `null` (default), no information will be logged.</remarks>
		public ILogger? Logger { get; init; }

		[System.Obsolete("Use constructor with arguments instead.", true)]
		public Config()
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Config(IPv4 ipv4, ushort port) : this(new Address(ipv4, port))
		{
		}

		public Config(Address address)
		{
			this.Address = address;
		}
	}
}
