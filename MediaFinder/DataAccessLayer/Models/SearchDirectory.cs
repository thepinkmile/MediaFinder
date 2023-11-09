namespace MediaFinder.DataAccessLayer.Models;

public class SearchDirectory
{
    public int Id { get; set; }

    public required string Path { get; set; }

    public int SettingsId { get; set; }
    public virtual SearchSettings? Settings { get; set; }
}
