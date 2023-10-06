using MediaFinder_v2.DataAccessLayer.Models;

namespace MediaFinder_v2.Services;

public interface IMediaDetector
{
    public MultiMediaType MediaType { get; }

    public bool IsPositiveDetection(string filepath, bool performDeepAnalysis);

    public IDictionary<string, string> GetMediaProperties(string filepath);
}
