using System.Runtime.CompilerServices;

namespace Byrone.Xenia.Example
{
	internal sealed class ConsoleLogger : ILogger
	{
		public void LogDebug(scoped System.ReadOnlySpan<byte> message)
		{
			System.Console.Error.Write("[Server] DEBUG: ");
			System.Console.Error.WriteLine(ConsoleLogger.Str(message));
		}

		public void LogInfo(scoped System.ReadOnlySpan<byte> message)
		{
			System.Console.Error.Write("[Server] INFO: ");
			System.Console.Error.WriteLine(ConsoleLogger.Str(message));
		}

		public void LogWarning(scoped System.ReadOnlySpan<byte> message)
		{
			System.Console.Error.Write("[Server] WARNING: ");
			System.Console.Error.WriteLine(ConsoleLogger.Str(message));
		}

		public void LogError(scoped System.ReadOnlySpan<byte> message)
		{
			System.Console.Error.Write("[Server] ERROR: ");
			System.Console.Error.WriteLine(ConsoleLogger.Str(message));
		}

		public void LogException(System.Exception ex)
		{
			System.Console.Error.Write("[Server] EXCEPTION: ");
			System.Console.Error.WriteLine(ex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string Str(scoped System.ReadOnlySpan<byte> message) =>
			System.Text.Encoding.UTF8.GetString(message);
	}
}
