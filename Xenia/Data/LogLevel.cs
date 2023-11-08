using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
    [PublicAPI]
    [System.Flags]
    public enum LogLevel
    {
        /// <summary>
        /// Don't write anything to the log.
        /// </summary>
        None = 0,

        /// <summary>
        /// Enable logging exceptions. It's highly recommended to keep this enabled.
        /// </summary>
        Exceptions = 1 << 0,

        /// <summary>
        /// Enable logging errors. Not that these don't include exceptions.
        /// </summary>
        Error = 1 << 1,

        /// <summary>
        /// Log exceptions and errors. This is the recommended setting in production environments.
        /// </summary>
        Errors = LogLevel.Exceptions | LogLevel.Error,

        /// <summary>
        /// Enable logging warnings.
        /// </summary>
        Warning = 1 << 2,

        /// <summary>
        /// Enable logging errors. It's recommended to only keep this enabled in development environments or when performance testing.
        /// </summary>
        Info = 1 << 3,

        /// <summary>
        /// Log everything. It's recommended to use this in development only.
        /// </summary>
        All = LogLevel.Errors | LogLevel.Warning | LogLevel.Info,
    }
}
