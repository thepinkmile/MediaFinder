namespace MediaFinder_v2.Messages;

public record WizardNavigationMessage(NavigationDirection NavigateTo)
{
    public static WizardNavigationMessage Create(NavigationDirection navigateTo)
        => new(navigateTo);
}
