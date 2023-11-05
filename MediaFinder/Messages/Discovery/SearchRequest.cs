using MediaFinder_v2.Helpers;
using MediaFinder_v2.Models;

namespace MediaFinder_v2.Messages;

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
