using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Helpers;

public record LogMessage(LogLevel LogLevel, Exception? Exception = null, string? Message = null, params string[] FormatArgs)
{
    public static LogMessage Log(LogLevel logLevel, string message, params string[] args)
        => new(logLevel, Message: message, FormatArgs: args);

    public static LogMessage Log(LogLevel logLevel, Exception exception, string message, params string[] args)
        => new(logLevel, exception, message, args);

    public static LogMessage LogTrace(string message, params string[] args)
        => new(LogLevel.Trace, Message: message, FormatArgs: args);

    public static LogMessage LogTrace(Exception exception, string message, params string[] args)
        => new(LogLevel.Trace, exception, message, args);

    public static LogMessage LogDebug(string message, params string[] args)
        => new(LogLevel.Debug, Message: message, FormatArgs: args);

    public static LogMessage LogDebug(Exception exception, string message, params string[] args)
        => new(LogLevel.Debug, exception, message, args);

    public static LogMessage LogInformation(string message, params string[] args)
        => new(LogLevel.Information, Message: message, FormatArgs: args);

    public static LogMessage LogInformation(Exception exception, string message, params string[] args)
        => new(LogLevel.Information, exception, message, args);

    public static LogMessage LogWarning(string message, params string[] args)
        => new(LogLevel.Warning, Message: message, FormatArgs: args);

    public static LogMessage LogWarning(Exception exception, string message, params string[] args)
        => new(LogLevel.Warning, exception, message, args);

    public static LogMessage LogError(string message, params string[] args)
        => new(LogLevel.Error, Message: message, FormatArgs: args);

    public static LogMessage LogError(Exception exception, string message, params string[] args)
        => new(LogLevel.Error, exception, message, args);

    public static LogMessage LogCritical(string message, params string[] args)
        => new(LogLevel.Critical, Message: message, FormatArgs: args);

    public static LogMessage LogCritical(Exception exception, string message, params string[] args)
        => new(LogLevel.Critical, exception, message, args);
}
