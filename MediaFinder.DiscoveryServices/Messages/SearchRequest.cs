using MediaFinder.Helpers;
using MediaFinder.Models;

namespace MediaFinder.Messages;

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

    public static SearchRequest Create(object progressToken, string workingDirectory, DiscoveryOptions options)
        => new(progressToken, workingDirectory, options.Directories,
            options.Recursive, options.ExtractArchives, options.ExtractionDepth);
}
