using Microsoft.Extensions.Logging;

namespace MediaFinder.Logging;

public static partial class ExportLoggingExtensions
{
    [LoggerMessage(4001, LogLevel.Information, "Exporting file: Original Path = {FilePath}, Destination = {OutputPath}")]
    public static partial void ExportingFile(this ILogger logger, string filePath, string outputPath);

    [LoggerMessage(4002, LogLevel.Debug, "Creating Directory: {Directory}")]
    public static partial void CreatingDirectory(this ILogger logger, string directory);

    [LoggerMessage(4003, LogLevel.Debug, "Export completed successfully")]
    public static partial void ExportComplete(this ILogger logger);

    [LoggerMessage(4004, LogLevel.Debug, "Potential file collision detected: Original = '{FilePath}', Actual = '{ResolvedFilePath}'")]
    public static partial void FilenameCollisionDetected(this ILogger logger, string filePath, string resolvedFilePath);
}
