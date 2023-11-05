namespace MediaFinder_v2.Messages;

public record FileExtracted(string Filenamaae)
{
    public static FileExtracted Create(string filename)
        => new(filename);
}
