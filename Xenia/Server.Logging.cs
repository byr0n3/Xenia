using System.Runtime.CompilerServices;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia
{
	public sealed partial class Server
	{
		private void Log(LogLevel level,
						 scoped System.Span<byte> buffer,
						 [InterpolatedStringHandlerArgument(nameof(buffer))]
						 scoped StringBuilder builder)
		{
			switch (level)
			{
				case LogLevel.Debug:
					this.config.Logger?.LogDebug(builder.Result);
					break;

				case LogLevel.Info:
					this.config.Logger?.LogInfo(builder.Result);
					break;

				case LogLevel.Warning:
					this.config.Logger?.LogWarning(builder.Result);
					break;

				case LogLevel.Error:
					this.config.Logger?.LogError(builder.Result);
					break;

				default:
					throw new System.ArgumentException("Unknown LogLevel", nameof(level));
			}
		}

		private enum LogLevel
		{
			Debug,
			Info,
			Warning,
			Error,
		}
	}
}
