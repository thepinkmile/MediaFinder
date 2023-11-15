namespace MediaFinder.Messages;

public record CancelProgressMessage(object Token)
{
    public static CancelProgressMessage Create(object token)
        => new(token);
}
