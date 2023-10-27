using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Helpers;

namespace MediaFinder_v2.Services.Search;

#region Discovery

public class SearchRequest : ReactiveBackgroundWorkerContextBase
{

    private SearchRequest(
        object progressToken,
        string workingDirectory,
        ICollection<string> sourceDirectories,
        bool recursive,
        bool extractArchives,
        int? extractionDepth)
        : base(progressToken)
    {
        WorkingDirectory = workingDirectory;
        SourceDirectories = sourceDirectories;
        Recursive = recursive;
        ExtractArchives = extractArchives;
        ExtractionDepth = extractionDepth;
    }

    public string WorkingDirectory { get; }

    public ICollection<string> SourceDirectories { get; }

    public bool Recursive { get; }

    public bool ExtractArchives { get; }

    public int? ExtractionDepth { get; }

    public static SearchRequest Create(object progressToken, string workingDirectory, SearchConfiguration settings)
        => new(progressToken, workingDirectory, settings.Directories,
            settings.Recursive, settings.ExtractArchives, settings.ExtractionDepth);
}

public record SearchResponse(ICollection<string> Files)
{
    public static SearchResponse Create(ICollection<string> files)
        => new(files);
}

public record WorkingDirectoryCreated(string Directory)
{
    public static WorkingDirectoryCreated Create(string directory)
        => new(directory);
}

#endregion

#region Analysis

public class AnalyseRequest : ReactiveBackgroundWorkerContextBase
{
    private AnalyseRequest(
        object progressToken,
        ICollection<string> files,
        ICollection<string> originalPaths,
        string workingDirectory,
        bool performDeepAnalysis = false)
        : base(progressToken)
    {
        Files = files;
        OriginalPaths = originalPaths;
        WorkingDirectory = workingDirectory;
        PerformDeepAnalysis = performDeepAnalysis;
    }

    public ICollection<string> Files { get; }

    public ICollection<string> OriginalPaths { get; }

    public string WorkingDirectory { get; }

    public bool PerformDeepAnalysis { get; }

    public static AnalyseRequest Create(object progressToken, ICollection<string> files,
        ICollection<string> originalPaths, string workingDirectory, bool performDeepAnalysis = false)
        => new(progressToken, files, originalPaths, workingDirectory, performDeepAnalysis);
}

public record AnalysisResponse(ICollection<FileDetails> Files)
{
    public static AnalysisResponse Create(ICollection<FileDetails> files)
        => new(files);
}

#endregion

#region Filtering

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

#endregion

public record SearchCompletedMessage
{
    public static SearchCompletedMessage Create()
        => new();
}