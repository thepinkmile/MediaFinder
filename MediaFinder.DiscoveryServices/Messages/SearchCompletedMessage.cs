namespace MediaFinder.Messages;

public record SearchCompletedMessage
{
    public static SearchCompletedMessage Create()
        => new();
}