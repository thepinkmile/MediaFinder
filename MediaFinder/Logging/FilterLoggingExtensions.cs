using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Logging;

public static partial class FilterLoggingExtensions
{
    [LoggerMessage(3001, LogLevel.Debug, "Portrait orientation detected: {dimensionType}")]
    public static partial void ProtraitOrientationDetected(this ILogger logger, string dimensionType);

    [LoggerMessage(3002, LogLevel.Debug, "Duplicate file hash located: MD5 = {MD5}, SHA256 = {SHA256}, SHA512 = {SHA512}, Count = {Count}")]
    public static partial void DuplicateChecksum(this ILogger logger, string? md5, string? sha256, string? sha512, int count);

    [LoggerMessage(3003, LogLevel.Information, "Media file filtered due to size: {Filename}")]
    public static partial void ExcludedBySize(this ILogger logger, string filename);
}
