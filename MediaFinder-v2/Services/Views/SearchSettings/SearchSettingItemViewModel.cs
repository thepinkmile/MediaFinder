using System.Drawing;

namespace MediaFinder_v2.Views.SearchSettings;

public class SearchSettingItemViewModel
{
    public int Id { get; }

    public string Name { get; }

    public string? Description { get; }

    public ICollection<string> Directories { get; }

    public bool Recursive { get; }

    public bool ExtractArchives { get; }

    public int? ExtractionDepth { get; }

    public bool PerformDeepAnalysis { get; }

    public string? WorkingDirectory { get; set; }

    public long? MinImageWidth { get; set; }

    public long? MinImageHeight { get; set; }

    public long? MinVideoWidth { get; set; }

    public long? MinVideoHeight { get; set; }

    public SearchSettingItemViewModel(DataAccessLayer.Models.SearchSettings item)
    {
        Id = item.Id;
        Name = item.Name;
        Description = item.Description;
        Directories = item.Directories.Select(x => x.Path).ToList();
        Recursive = item.Recursive;
        ExtractArchives = item.ExtractArchives;
        ExtractionDepth = item.ExtractionDepth;
        PerformDeepAnalysis = item.PerformDeepAnalysis;
        MinImageHeight = item.MinImageHeight;
        MinImageWidth = item.MinImageWidth;
        MinVideoHeight = item.MinVideoHeight;
        MinVideoWidth = item.MinVideoWidth;
    }
}
