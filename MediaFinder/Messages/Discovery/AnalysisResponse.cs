using MediaFinder.DataAccessLayer.Models;

namespace MediaFinder.Messages;

public record AnalysisResponse(ICollection<FileDetails> Files)
{
    public static AnalysisResponse Create(ICollection<FileDetails> files)
        => new(files);
}
