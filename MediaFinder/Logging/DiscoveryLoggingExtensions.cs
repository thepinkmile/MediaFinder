using Microsoft.Extensions.Logging;

using SevenZipExtractor;

namespace MediaFinder_v2.Logging;

public static partial class DiscoveryLoggingExtensions
{
    [LoggerMessage(1001, LogLevel.Debug, "Working directory created: {WorkingDirectory}")]
    public static partial void CreatedWorkingDirectory(this ILogger logger, string workingDirectory);

    [LoggerMessage(1002, LogLevel.Information, "Iterating files in directory: {Directory}")]
    public static partial void IteratingDirectory(this ILogger logger, string directory);

    [LoggerMessage(1003, LogLevel.Debug, "Archive extracted to: {ExtractionPath}")]
    public static partial void ArchiveExtracted(this ILogger logger, string extractionPath);

    [LoggerMessage(1004, LogLevel.Debug, "Performing archive detection: {FilePath}")]
    public static partial void PerformingArchiveDetection(this ILogger logger, string filePath);

    [LoggerMessage(1005, LogLevel.Warning, "Extraction path already exists: {ExtractionPath}")]
    public static partial void ExtractionPathExists(this ILogger logger, string extractionPath);

    [LoggerMessage(1006, LogLevel.Debug, "Extracting Archive: {FilePath}")]
    public static partial void PerformingArchiveExtraction(this ILogger logger, string filePath);

    [LoggerMessage(1007, LogLevel.Information, "Archive detection failed for {FilePath}")]
    public static partial void ArchiveExtractionFailed(this ILogger logger, SevenZipException ex, string filePath);

    [LoggerMessage(1008, LogLevel.Debug, "Skipping known non archive file: {FilePath}")]
    public static partial void KnownNonArchive(this ILogger logger, string filePath);

    [LoggerMessage(1009, LogLevel.Information, "Archive extraction failed for {FilePath}")]
    public static partial void ArchiveExtractionFailed(this ILogger logger, Exception ex, string filePath);
}
