namespace MediaFinder_v2.Views.SearchSettings;

public class SearchSettingItemViewModel
{
    public int Id { get; }

    public string Name { get; }

    public string? Description { get; }

    public ICollection<string> Directories { get; }

    public bool Recursive { get; }

    public bool ExtractArchives { get; }

    public bool PerformDeepAnalysis { get; }

    public SearchSettingItemViewModel(DataAccessLayer.Models.SearchSettings item)
    {
        Id = item.Id;
        Name = item.Name;
        Description = item.Description;
        Directories = item.Directories.Select(x => x.Path).ToList();
        Recursive = item.Recursive;
        ExtractArchives = item.ExtractArchives;
        PerformDeepAnalysis = item.PerformDeepAnalysis;
    }
}
