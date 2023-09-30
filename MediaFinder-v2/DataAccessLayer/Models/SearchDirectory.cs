namespace MediaFinder_v2.DataAccessLayer.Models;

public class SearchDirectory
{
    public int Id { get; set; }

    public required string Path { get; set; }

    public virtual SearchSettings Settings { get; set; } = null!;
}
