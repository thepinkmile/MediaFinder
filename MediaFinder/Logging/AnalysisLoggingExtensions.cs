using Microsoft.Extensions.Logging;

namespace MediaFinder.Logging;

public static partial class AnalysisLoggingExtensions
{
    [LoggerMessage(2001, LogLevel.Information, "Analysing file: {FilePath}")]
    public static partial void AnalysingFile(this ILogger logger, string filePath);

    [LoggerMessage(2002, LogLevel.Debug, "Compilinig analysis result: {FilePath}")]
    public static partial void CompilingResults(this ILogger logger, string filePath);

    [LoggerMessage(2003, LogLevel.Error, "Failed to describe file: {Filename}")]
    public static partial void FailedToDescribeFile(this ILogger logger, Exception ex, string filename);

    [LoggerMessage(2004, LogLevel.Debug, "Performing file extension video detection: {FilePath}")]
    public static partial void PerformingVideoDetection(this ILogger logger, string filePath);

    [LoggerMessage(2005, LogLevel.Debug, "Video detected: {FilePath}")]
    public static partial void VideoDetected(this ILogger logger, string filePath);

    [LoggerMessage(2006, LogLevel.Debug, "Audio detected: {FilePath}")]
    public static partial void AudioDetected(this ILogger logger, string filePath);

    [LoggerMessage(2007, LogLevel.Debug, "Failed to analyse file as video: {FilePath}")]
    public static partial void NoVideoDetected(this ILogger logger, string filePath, Exception? ex = null);

    [LoggerMessage(2008, LogLevel.Warning, "Failed to parse date of audio: File = '{FilePath}', Date = '{DateFormat}'")]
    public static partial void FailedToParseAudioDate(this ILogger logger, string filePath, string dateFormat);

    [LoggerMessage(2009, LogLevel.Debug, "Performing video metadata detection: {FilePath}")]
    public static partial void PerformingVideoMetadataDetection(this ILogger logger, string filePath);

    [LoggerMessage(2010, LogLevel.Warning, "No video metadata detected: {FilePath}")]
    public static partial void FFProbeFailure(this ILogger logger, Exception ex, string filePath);

    [LoggerMessage(2011, LogLevel.Debug, "Performing file extension image detection: {FilePath}")]
    public static partial void PerformingImageDetection(this ILogger logger, string filePath);

    [LoggerMessage(2012, LogLevel.Debug, "Image detected: {FilePath}")]
    public static partial void ImageDetected(this ILogger logger, string filePath);

    [LoggerMessage(2013, LogLevel.Warning, "Failed to analyse file as image: {FilePath}")]
    public static partial void NoImageDetected(this ILogger logger, string filePath, Exception? ex = null);

    [LoggerMessage(2014, LogLevel.Debug, "Performing image metadata detection: {FilePath}")]
    public static partial void PerformingImageMetadataDetection(this ILogger logger, string filePath);

    [LoggerMessage(2015, LogLevel.Warning, "Failed to parse dimension property: Detail = '{DetailProperty}', PropertyName = '{PropertyName}', PropertyValue = '{PropertyValue}'")]
    public static partial void FailedToParseDimensionDetail(this ILogger logger, string detailProperty, string propertyName, string propertyValue);

    [LoggerMessage(2016, LogLevel.Debug, "Skipping basic metadata analysis: {FilePath}")]
    public static partial void SkippingBasicMetadataAnalysis(this ILogger logger, string filePath);

    [LoggerMessage(2017, LogLevel.Debug, "Performing basic metadata detection: {FilePath}")]
    public static partial void PerformingMetadataDetection(this ILogger logger, string filePath);

    [LoggerMessage(2018, LogLevel.Debug, "Unknown media type - skipping metadata retrieval: {FilePath}")]
    public static partial void UnknownMediaType(this ILogger logger, string filePath);

    [LoggerMessage(2019, LogLevel.Information, "Text file detected - skipping: {FilePath}")]
    public static partial void TextMediaTypeDetected(this ILogger logger, string filePath);

    [LoggerMessage(2020, LogLevel.Warning, "Failed to analyse file metadata: {FilePath}")]
    public static partial void FailedToReadMetadata(this ILogger logger, Exception ex, string filePath);

    [LoggerMessage(2021, LogLevel.Debug, "Creating file checksum: {FilePath}")]
    public static partial void CreatingFileChecksum(this ILogger logger, string filePath);

    [LoggerMessage(2022, LogLevel.Error, "Failed to generate checksum for file: {FilePath}")]
    public static partial void FailedToCreateChecksum(this ILogger logger, Exception ex, string filePath);
}
