using System.ComponentModel.DataAnnotations;

namespace MediaFinder.Models;

public class DiscoveryOptions
{
    public int Id { get; init; }

    [Required]
    public required string Name { get; init; }

    public string? Description { get; init; }

    public required ICollection<string> Directories { get; init; } = new List<string>();

    public bool Recursive { get; set; }

    public bool ExtractArchives { get; set; }

    public int? ExtractionDepth { get; set; }

    public bool PerformDeepAnalysis { get; set; }

    public string? WorkingDirectory { get; set; }

    public long? MinImageWidth { get; set; }

    public long? MinImageHeight { get; set; }

    public long? MinVideoWidth { get; set; }

    public long? MinVideoHeight { get; set; }
}
