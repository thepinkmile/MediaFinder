using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;

using MediaFinder_v2.Messages;
using MediaFinder_v2.Views.Discovery;
using MediaFinder_v2.Views.Export;
using MediaFinder_v2.Views.Status;

namespace MediaFinder_v2.Views;

public partial class MainWindowsViewModel : ObservableObject,
    IRecipient<WizardNavigationMessage>,
    IRecipient<SnackBarMessage>,
    IRecipient<ShowProgressMessage>,
    IRecipient<UpdateProgressMessage>,
    IRecipient<CancelProgressMessage>,
    IRecipient<CompleteProgressMessage>,
    IRecipient<FinishedMessage>
{
    private readonly IMessenger _messenger;

    public MainWindowsViewModel(
        IMessenger messenger,
        ISnackbarMessageQueue snackbarMessageQueue,
        DiscoveryViewModel discoveryViewModel,
        ExportViewModel exportViewModel,
        ProcessCompletedViewModel statusViewModel)
    {
        _messenger = messenger;
        
        _messenger.RegisterAll(this);

        MessageQueue = snackbarMessageQueue;
        DiscoveryViewModel = discoveryViewModel;
        ExportViewModel = exportViewModel;
        StatusViewModel = statusViewModel;
    }

    public DiscoveryViewModel DiscoveryViewModel { get; }

    public ExportViewModel ExportViewModel { get; }

    public ProcessCompletedViewModel StatusViewModel { get; }

    #region Navigation

    public void Receive(WizardNavigationMessage message)
    {
        switch (message.NavigateTo)
        {
            case NavigationDirection.Next: Transitioner.MoveNextCommand.Execute(null, null); break;
            case NavigationDirection.Previous: Transitioner.MovePreviousCommand.Execute(null, null); break;
            case NavigationDirection.Beginning: Transitioner.MoveFirstCommand.Execute(null, null); break;
            case NavigationDirection.End: Transitioner.MoveLastCommand.Execute(null, null); break;
            default: break;
        }
    }

    #endregion

    #region SnackBar

    public ISnackbarMessageQueue MessageQueue { get; }

    public void Receive(SnackBarMessage message)
    {
        MessageQueue.Enqueue(message.Message);
    }

    #endregion

    #region ProgressOverlay

    [ObservableProperty]
    private ICommand? _progressCancelCommand;

    [ObservableProperty]
    private bool _progressVisible;

    [ObservableProperty]
    private string? _progressMessage;

    [ObservableProperty]
    private int _progressValue;

    private object? _progressToken;

    public void Receive(ShowProgressMessage message)
    {
        if (_progressToken is not null && _progressToken != message.Token)
        {
            throw new InvalidOperationException("Progress is already visible for an alternative token");
        }

        _progressToken = message.Token;
        ProgressCancelCommand = message.CancelCommand;
        ProgressValue = message.Progress;
        ProgressMessage = message.Message;
        ProgressVisible = true;
    }

    public void Receive(UpdateProgressMessage message)
    {
        if (_progressToken is not null && _progressToken != message.Token)
        {
            throw new InvalidOperationException("Progress is already visible for an alternative token");
        }

        _progressToken = message.Token;
        ProgressValue = message.Progress;
        ProgressMessage = message.Message;
        ProgressVisible = true;
    }

    public void Receive(CancelProgressMessage message)
    {
        if (_progressToken is not null && _progressToken != message.Token)
        {
            throw new InvalidOperationException("Progress is already visible for an alternative token");
        }

        _progressToken = message.Token;
        ProgressValue = 0;
        ProgressMessage = "Cancelling...";
        ProgressCancelCommand = null;
    }

    public void Receive(CompleteProgressMessage message)
    {
        if (_progressToken is not null && _progressToken != message.Token)
        {
            throw new InvalidOperationException("Progress is already visible for an alternative token");
        }

        _progressToken = null;
        ProgressCancelCommand = null;
        ProgressValue = 0;
        ProgressMessage = null;
        ProgressVisible = false;
    }

    #endregion

    #region Finished

    public void Receive(FinishedMessage message)
    {
        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Beginning));
    }

    #endregion
}
