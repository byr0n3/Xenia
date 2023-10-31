using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	public interface IServerLogger
	{
		public void LogInfo(string message);

		public void LogWarning(string message);

		public void LogError(string message);

		public void LogException(System.Exception ex, string message);
	}
}
