using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Logging;

public static partial class GeneralLoggingExtensions
{
    [LoggerMessage(EventId = 0, Message = "{Message}")]
    public static partial void Log(this ILogger logger, string message, LogLevel logLevel = LogLevel.Debug);

    [LoggerMessage(600, LogLevel.Information, "User cancelled operation: {Operation}")]
    public static partial void UserCanceledOperation(this ILogger logger, string operation);
}
