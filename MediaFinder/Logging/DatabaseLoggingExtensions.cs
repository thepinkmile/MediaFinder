using Microsoft.Extensions.Logging;

namespace MediaFinder.Logging;

public static partial class DatabaseLoggingExtensions
{
    [LoggerMessage(9001, LogLevel.Error, "Failed to update database ({ErrorType})")]
    public static partial void DatabaseError(this ILogger logger, Exception ex, string errorType);

    [LoggerMessage(9002, LogLevel.Error, "Failed to update database")]
    public static partial void DatabaseError(this ILogger logger, Exception ex);
}
