using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2.Services.Search;

public record SearchRequest(string WorkingDirectory, ICollection<string> SourceDirectories, bool Recursive, bool ExtractArchives, int? ExtractionDepth)
{
    public static SearchRequest Create(string workingDirectory, SearchSettingItemViewModel settings)
        => new(workingDirectory, settings.Directories, settings.Recursive, settings.ExtractArchives, settings.ExtractionDepth);
}

public record SearchResponse(ICollection<string> Files)
{
    public static SearchResponse Create(ICollection<string> files)
        => new(files);
}

public record WorkingDirectoryCreated(string Path)
{
    public static WorkingDirectoryCreated Create(string path)
        => new(path);
}

public record AnalyseRequest(ICollection<string> Files, ICollection<string> OriginalPaths, string WorkingDirectory, bool PerformDeepAnalysis = false)
{
    public static AnalyseRequest Create(ICollection<string> files, ICollection<string> originalPaths, string workingDirectory, bool performDeepAnalysis = false)
        => new(files, originalPaths, workingDirectory, performDeepAnalysis);
}

public record AnalysisResponse(ICollection<FileDetails> Files)
{
    public static AnalysisResponse Create(ICollection<FileDetails> files)
        => new(files);
}

public record FilterRequest(long? MinImageWidth, long? MinImageHeight, long? MinVideoWidth, long? MinVideoHeight)
{
    public static FilterRequest Create(long? minImageWidth, long? minImageHeight, long? minVideoWidth, long? minVideoHeight)
        => new(minImageWidth, minImageHeight, minVideoWidth, minVideoHeight);
}