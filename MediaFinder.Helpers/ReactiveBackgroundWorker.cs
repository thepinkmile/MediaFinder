using System.ComponentModel;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder.Logging;
using MediaFinder.Messages;

using Microsoft.Extensions.Logging;

namespace MediaFinder.Helpers;

public abstract class ReactiveBackgroundWorker<T> : BackgroundWorker
    where T : ReactiveBackgroundWorkerContextBase
{
    protected readonly ILogger _logger;

    private readonly IMessenger _messenger;

    private object? _progressToken;

    protected ReactiveBackgroundWorker(ILogger logger, IMessenger messenger)
    {
        _logger = logger;
        _messenger = messenger;

        WorkerReportsProgress = true;
        WorkerSupportsCancellation = true;
        ProgressChanged += UpdateProgress;
        DoWork += Execute;
    }

    #region Background Methods

    protected void Execute(object? sender, DoWorkEventArgs e)
    {
        if (e.Argument is not T inputs || inputs is null)
        {
            throw new InvalidOperationException("Stage called with invalid arguments.");
        }

        _progressToken = inputs.ProgressToken;
        Execute(inputs, e);
    }

    protected abstract void Execute(T context, DoWorkEventArgs e);

    #region ProgressReporting

    protected void ReportProgress(object userState)
        => ReportProgress(default, userState);

    protected void SetProgress(string message, LogLevel logLevel = LogLevel.Debug)
    {
        ReportProgress(UpdateProgressMessage.Create(_progressToken!, message));
        _logger.Message(message, logLevel);
    }

    #endregion

    #endregion

    #region UI Methods

    protected virtual void UpdateProgress(object? sender, ProgressChangedEventArgs e)
    {
        switch (e.UserState)
        {
            case string stateMessage: _messenger.Send(UpdateProgressMessage.Create(_progressToken!, stateMessage)); break;
            case UpdateProgressMessage updateStatusMessage: _messenger.Send(updateStatusMessage); break;
            default:
                if (e.UserState is not null)
                    _messenger.Send(e.UserState);
                break;
        }
    }

    #endregion
}
