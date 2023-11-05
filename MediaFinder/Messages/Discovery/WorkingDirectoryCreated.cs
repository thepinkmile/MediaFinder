namespace MediaFinder_v2.Messages;

public record WorkingDirectoryCreated(string Directory)
{
    public static WorkingDirectoryCreated Create(string directory)
        => new(directory);
}
