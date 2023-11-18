using Microsoft.Extensions.Logging;

namespace MediaFinder.Logging;

public static partial class GeneralLoggingExtensions
{
    [LoggerMessage(EventId = 0, Message = "{Message}")]
    public static partial void Message(this ILogger logger, string message, LogLevel logLevel = LogLevel.Debug);

    [LoggerMessage(600, LogLevel.Information, "User cancelled operation: {Operation}")]
    public static partial void UserCanceledOperation(this ILogger logger, string operation);

    [LoggerMessage(200, LogLevel.Debug, "ProgressUpdate: {ProgressMessage}")]
    public static partial void ProgressUpdate(this ILogger logger, string progressMessage);

    [LoggerMessage(800, LogLevel.Error, "Process failed.")]
    public static partial void ProcessFailed(this ILogger logger, Exception? ex = null);

    [LoggerMessage(801, LogLevel.Error, "Invalid process result: Process = {Process}, Result Type = {ResultType}")]
    public static partial void InvalidResult(this ILogger logger, string process, Type resultType);

    [LoggerMessage(201, LogLevel.Information, "Cleaning up search session")]
    public static partial void SessionCleanup(this ILogger logger);

    [LoggerMessage(202, LogLevel.Debug, "Removed Working Directory: {WorkingDirectory}")]
    public static partial void RemovedWorkingDirectory(this ILogger logger, string workingDirectory);

    [LoggerMessage(850, LogLevel.Error, "An unhandled exception occured.")]
    public static partial void UnhandledException(this ILogger logger, Exception? ex = null);
}
