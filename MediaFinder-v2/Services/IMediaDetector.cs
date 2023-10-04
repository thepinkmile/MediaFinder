namespace MediaFinder_v2.Services;

public interface IMediaDetector
{
    public bool IsPositiveDetection(string filepath);

    public IDictionary<string, string> GetMediaProperties(string filepath);
}
