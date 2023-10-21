using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using MediaFinder_v2.Messages;
using MediaFinder_v2.Views.Executors;

namespace MediaFinder_v2.Views;

public partial class MainWindowsViewModel : ObservableObject,
    IRecipient<SnackBarMessage>,
    IRecipient<ShowProgressMessage>,
    IRecipient<UpdateProgressMessage>,
    IRecipient<CancelProgressMessage>,
    IRecipient<CompleteProgressMessage>
{
    public MainWindowsViewModel(
        IMessenger messenger,
        ISnackbarMessageQueue snackbarMessageQueue,
        SearchExecutorViewModel searchExecutorViewModel)
    {
        messenger.RegisterAll(this);
        MessageQueue = snackbarMessageQueue;
        _searchExecutorViewModel = searchExecutorViewModel;
    }

    [ObservableProperty]
    private SearchExecutorViewModel _searchExecutorViewModel;

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
}
