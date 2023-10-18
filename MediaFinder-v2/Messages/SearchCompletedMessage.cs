namespace MediaFinder_v2.Messages;

public record SearchCompletedMessage
{
    public static SearchCompletedMessage Create()
        => new();
}
