namespace Byrone.Xenia.Example
{
	internal sealed class ConsoleLogger : IServerLogger
	{
		public void LogInfo(string message) =>
			System.Console.WriteLine("[Server] " + message);

		public void LogWarning(string message) =>
			System.Console.WriteLine("[Server] " + message);

		public void LogError(string message) =>
			System.Console.WriteLine("[Server] " + message);

		public void LogException(System.Exception ex, string message)
		{
			System.Console.Error.WriteLine("[Server] " + message);
			System.Console.Error.WriteLine(ex);
		}
	}
}
