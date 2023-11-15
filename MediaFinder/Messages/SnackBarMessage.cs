namespace MediaFinder.Messages;

public record SnackBarMessage(string Message)
{
    public static SnackBarMessage Create(string message)
        => new(message);
}
