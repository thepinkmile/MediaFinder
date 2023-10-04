namespace MediaFinder_v2.Services;

public record ExtractionState(string Source, string Destination)
{
    public static ExtractionState Create(string source, string destination)
        => new(source, destination);
}
