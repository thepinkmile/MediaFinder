namespace MediaFinder.Models;

public class SearchConfiguration
{
    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public ICollection<string> Directories { get; private set; } = null!;

    public bool Recursive { get; private set; }

    public bool ExtractArchives { get; private set; }

    public int? ExtractionDepth { get; private set; }

    public bool PerformDeepAnalysis { get; private set; }

    public string? WorkingDirectory { get; set; }

    public long? MinImageWidth { get; private set; }

    public long? MinImageHeight { get; private set; }

    public long? MinVideoWidth { get; private set; }

    public long? MinVideoHeight { get; private set; }

    public static SearchConfiguration Create(DataAccessLayer.Models.SearchSettings item)
        => new()
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Directories = item.Directories.Select(x => x.Path).ToList(),
            Recursive = item.Recursive,
            ExtractArchives = item.ExtractArchives,
            ExtractionDepth = item.ExtractionDepth,
            PerformDeepAnalysis = item.PerformDeepAnalysis,
            MinImageHeight = item.MinImageHeight,
            MinImageWidth = item.MinImageWidth,
            MinVideoHeight = item.MinVideoHeight,
            MinVideoWidth = item.MinVideoWidth
        };
}
