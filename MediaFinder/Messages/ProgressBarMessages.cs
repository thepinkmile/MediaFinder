namespace MediaFinder_v2.Messages;

public record class ShowProgressBar(string? Message)
{
    public static ShowProgressBar Create(string? message = null)
        => new(message);
}

public record class HideProgressBar()
{
    public static HideProgressBar Create()
        => new();
}

public record class UpdateProgressBarStatus(string? Message)
{
    public static UpdateProgressBarStatus Create(string? message = null)
        => new(message);
}

public record class UpdateProgressBarValue(int Value)
{
    public static UpdateProgressBarValue Create(int value = -1)
        => new(value);
}