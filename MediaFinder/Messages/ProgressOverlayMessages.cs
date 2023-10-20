using System.Windows.Input;

namespace MediaFinder_v2.Messages
{
    public record ShowProgressMessage(object Token, int Progress = 0, string? Message = null, ICommand? CancelCommand = null)
    {
        public static ShowProgressMessage Create(object token, string message, ICommand? cancelCommanad = null)
            => Create(token, 0, message, cancelCommanad);
        public static ShowProgressMessage Create(object token, int progress, ICommand? cancelCommanad = null)
            => new(token, progress, CancelCommand: cancelCommanad);

        public static ShowProgressMessage Create(object token, int progress, string message, ICommand? cancelCommanad = null)
            => new(token, progress, message, cancelCommanad);
    }

    public record UpdateProgressMessage(object Token, int Progress = 0, string? Message = null)
    {
        public static UpdateProgressMessage Create(object token, int progress)
            => new(token, progress);

        public static UpdateProgressMessage Create(object token, string message)
            => new(token, Message: message);

        public static UpdateProgressMessage Create(object token, int progress, string message)
            => new(token, progress, message);
    }

    public record CancelProgressMessage(object Token)
    {
        public static CancelProgressMessage Create(object token)
            => new(token);
    }

    public record CompleteProgressMessage(object Token)
    {
        public static CompleteProgressMessage Create(object token)
            => new(token);
    }
}
