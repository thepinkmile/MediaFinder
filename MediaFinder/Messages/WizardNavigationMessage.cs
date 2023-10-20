namespace MediaFinder_v2.Messages;

public enum NavigationDirection
{
    Next,

    Previous,

    Beginning,

    End
}

public record WizardNavigationMessage(NavigationDirection NavigateTo)
{
    public static WizardNavigationMessage Create(NavigationDirection navigateTo)
        => new(navigateTo);
}
