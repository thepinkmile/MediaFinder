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

public record AnalyseRequest(ICollection<string> Files, bool PerformDeepAnalysis = false)
{
    public static AnalyseRequest Creatae(ICollection<string> files, bool performDeepAnalysis = false)
        => new(files, performDeepAnalysis);
}

public record AnalysisResponse(ICollection<FileDetails> Files)
{
    public static AnalysisResponse Create(ICollection<FileDetails> files)
        => new(files);


    public static AnalysisResponse Create()
        => new(new List<FileDetails>());
}