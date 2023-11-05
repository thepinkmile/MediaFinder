using MediaFinder_v2.DataAccessLayer.Models;

namespace MediaFinder_v2.Messages;

public record AnalysisResponse(ICollection<FileDetails> Files)
{
    public static AnalysisResponse Create(ICollection<FileDetails> files)
        => new(files);
}
