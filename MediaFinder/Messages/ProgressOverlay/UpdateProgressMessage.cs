namespace MediaFinder_v2.Messages;

public record UpdateProgressMessage(object Token, int Progress = 0, string? Message = null)
{
    public static UpdateProgressMessage Create(object token, int progress)
        => new(token, progress);

    public static UpdateProgressMessage Create(object token, string message)
        => new(token, Message: message);

    public static UpdateProgressMessage Create(object token, int progress, string message)
        => new(token, progress, message);
}
