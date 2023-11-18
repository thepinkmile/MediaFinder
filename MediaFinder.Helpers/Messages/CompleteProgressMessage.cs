namespace MediaFinder.Messages;

public record CompleteProgressMessage(object Token)
{
    public static CompleteProgressMessage Create(object token)
        => new(token);
}
