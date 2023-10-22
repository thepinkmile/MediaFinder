using System.ComponentModel.DataAnnotations;

namespace MediaFinder_v2.DataAccessLayer.Models;

public record SearchSettings
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<SearchDirectory> Directories { get; set; } = null!;

    public bool Recursive { get; set; }

    public bool ExtractArchives { get; set; }

    [Range(0, 20)]
    public int? ExtractionDepth { get; set; }

    public bool PerformDeepAnalysis { get; set; }

    public long? MinImageWidth { get; set; }

    public long? MinImageHeight { get; set; }

    public long? MinVideoWidth {  get; set; }

    public long? MinVideoHeight { get; set; }
}
