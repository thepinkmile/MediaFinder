using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using MediaFinder_v2.Messages;

namespace MediaFinder_v2.Views;

public partial class MainWindowsViewModel : ObservableObject,
    IRecipient<ChangeTab>,
    IRecipient<ShowProgressBar>,
    IRecipient<HideProgressBar>,
    IRecipient<UpdateProgressBarStatus>,
    IRecipient<SnackBarMessage>
{
    [ObservableProperty]
    private int _selectedTabIndex = 1;

    [ObservableProperty]
    private bool _progressBarVisible;

    [ObservableProperty]
    private string? _progressBarStatus;

    public ISnackbarMessageQueue MessageQueue { get; }

    public MainWindowsViewModel(IMessenger messenger, ISnackbarMessageQueue snackbarMessageQueue)
    {
        messenger.RegisterAll(this);
        MessageQueue = snackbarMessageQueue;
    }

    public void Receive(ChangeTab message)
    {
        SelectedTabIndex = message.TabIndex;
    }

    public void Receive(UpdateProgressBarStatus message)
    {
        ProgressBarStatus = message.Message;
    }

    public void Receive(HideProgressBar message)
    {
        ProgressBarVisible = false;
        ProgressBarStatus = null;
    }

    public void Receive(ShowProgressBar message)
    {
        ProgressBarStatus = message.Message;
        ProgressBarVisible = true;
    }

    public void Receive(SnackBarMessage message)
    {
        MessageQueue.Enqueue(message.Message);
    }
}
