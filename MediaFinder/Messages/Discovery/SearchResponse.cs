namespace MediaFinder.Messages;

public record SearchResponse(ICollection<string> Files)
{
    public static SearchResponse Create(ICollection<string> files)
        => new(files);
}
