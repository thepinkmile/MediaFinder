using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Helpers;

public record LogMessage(LogLevel LogLevel, string Message, Exception? Exception = null)
{
    public static LogMessage Log(LogLevel logLevel, string message, Exception? exception = null)
        => new(logLevel, message, exception);

    public static LogMessage LogTrace(string message, Exception? exception = null)
        => new(LogLevel.Trace, message, exception);

    public static LogMessage LogDebug(string message, Exception? exception = null)
        => new(LogLevel.Debug, message, exception);

    public static LogMessage LogInformation(string message, Exception? exception = null)
        => new(LogLevel.Information, message, exception);

    public static LogMessage LogWarning(string message, Exception? exception = null)
        => new(LogLevel.Warning, message, exception);

    public static LogMessage LogError(string message, Exception? exception = null)
        => new(LogLevel.Error, message, exception);

    public static LogMessage LogCritical(string message, Exception? exception = null)
        => new(LogLevel.Critical, message, exception);
}
