namespace MediaFinder_v2.Messages;

public record CancelProgressMessage(object Token)
{
    public static CancelProgressMessage Create(object token)
        => new(token);
}
