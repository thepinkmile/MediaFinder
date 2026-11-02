namespace MediaFinder.Messages;

public record DiscoveryCompletedMessage
{
    public static DiscoveryCompletedMessage Create()
        => new();
}