using MediaFinder.Helpers;

namespace MediaFinder.Messages;

public class FilterRequest : ReactiveBackgroundWorkerContextBase
{
    private FilterRequest(
        object progressToken,
        long? minImageWidth,
        long? minImageHeight,
        long? minVideoWidth,
        long? minVideoHeight)
        : base(progressToken)
    {
        MinImageWidth = minImageWidth;
        MinImageHeight = minImageHeight;
        MinVideoWidth = minVideoWidth;
        MinVideoHeight = minVideoHeight;
    }

    public long? MinImageWidth { get; }

    public long? MinImageHeight { get; }

    public long? MinVideoWidth { get; }

    public long? MinVideoHeight { get; }

    public static FilterRequest Create(object progressToken, long? minImageWidth, long? minImageHeight,
        long? minVideoWidth, long? minVideoHeight)
        => new(progressToken, minImageWidth, minImageHeight, minVideoWidth, minVideoHeight);
}
