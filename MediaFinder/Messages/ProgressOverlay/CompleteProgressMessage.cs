namespace MediaFinder_v2.Messages;

public record CompleteProgressMessage(object Token)
{
    public static CompleteProgressMessage Create(object token)
        => new(token);
}
