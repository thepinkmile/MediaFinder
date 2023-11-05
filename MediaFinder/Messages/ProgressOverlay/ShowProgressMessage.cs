using System.Windows.Input;

namespace MediaFinder_v2.Messages;

public record ShowProgressMessage(object Token, int Progress = 0, string? Message = null, ICommand? CancelCommand = null)
{
    public static ShowProgressMessage Create(object token, string message, ICommand? cancelCommanad = null)
        => Create(token, 0, message, cancelCommanad);
    public static ShowProgressMessage Create(object token, int progress, ICommand? cancelCommanad = null)
        => new(token, progress, CancelCommand: cancelCommanad);

    public static ShowProgressMessage Create(object token, int progress, string message, ICommand? cancelCommanad = null)
        => new(token, progress, message, cancelCommanad);
}
