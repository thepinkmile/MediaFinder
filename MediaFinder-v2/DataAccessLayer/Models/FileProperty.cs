namespace MediaFinder_v2.DataAccessLayer.Models;

public class FileProperty
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Value { get; set; }

    public int FileDetailsId { get; set; }
    public virtual FileDetails? FileDetails { get; set; }
}
