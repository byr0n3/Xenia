namespace Byrone.Xenia.Example
{
	internal sealed class ConsoleLogger : IServerLogger
	{
		public void LogInfo(string message) =>
			System.Console.WriteLine(message);

		public void LogWarning(string message) =>
			System.Console.WriteLine(message);

		public void LogError(string message) =>
			System.Console.WriteLine(message);

		public void LogException(System.Exception ex, string message) =>
			throw ex;
	}
}
