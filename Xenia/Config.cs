using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Utilities;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
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

		/// <summary>
		/// The time to wait for a response, in microseconds.
		/// </summary>
		/// <remarks>You could technically put this to '0' to not wait for additional request data, however this <i>could</i> cause loss-of-data.</remarks>
		public int PollInterval { get; init; } = 100;

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
