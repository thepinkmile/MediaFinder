using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Logging;

public static partial class FilterLoggingExtensions
{
    [LoggerMessage(3001, LogLevel.Debug, "Portrait orientation detected: {dimensionType}")]
    public static partial void ProtraitOrientationDetected(this ILogger logger, string dimensionType);
}
