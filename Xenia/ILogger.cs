using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	public interface ILogger
	{
		/// <summary>
		/// Log messages used for debugging purposes. (client connected, request size, etc.)
		/// </summary>
		/// <param name="message">The message to log.</param>
		/// <remarks>You normally don't need to log these unless you're debugging issues.</remarks>
		public void LogDebug(scoped System.ReadOnlySpan<byte> message);

		/// <summary>
		/// Log messages used for informational purposes. (server started, handled request, timings, etc.)
		/// </summary>
		/// <param name="message">The message to log.</param>
		/// <remarks>These messages could be useful to have.</remarks>
		public void LogInfo(scoped System.ReadOnlySpan<byte> message);

		/// <summary>
		/// Log messages used to warn for certain events. (unexpected request, slow response, etc.)
		/// </summary>
		/// <param name="message">The message to log.</param>
		/// <remarks>A warning log doesn't have to mean something's wrong, but could warn you for further issues.</remarks>
		public void LogWarning(scoped System.ReadOnlySpan<byte> message);

		/// <summary>
		/// Logs error messages. (invalid response, etc.)
		/// </summary>
		/// <param name="message">The message to log.</param>
		/// <remarks>This doesn't include Exceptions, see <seealso cref="LogException"/>.</remarks>
		public void LogError(scoped System.ReadOnlySpan<byte> message);

		/// <summary>
		/// Logs specifically exceptions.
		/// </summary>
		/// <param name="ex">The exception to log.</param>
		public void LogException(System.Exception ex);
	}
}
