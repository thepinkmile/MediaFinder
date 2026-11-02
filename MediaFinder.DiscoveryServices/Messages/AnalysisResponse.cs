using MediaFinder.Models;

namespace MediaFinder.Messages;

public record AnalysisResponse(ICollection<MediaFile> Files)
{
    public static AnalysisResponse Create(ICollection<MediaFile> files)
        => new(files);
}
