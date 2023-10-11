using System.ComponentModel;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.Messages;

using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Helpers;

public abstract class ReactiveBackgroundWorker : BackgroundWorker
{
    private readonly ILogger _logger;
    private readonly IMessenger _messenger;

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

    protected abstract void Execute(object? sender, DoWorkEventArgs e);

    protected void ReportProgress(object userState)
        => ReportProgress(default, userState);

    #region Logging

    protected void Log(LogLevel logLevel, string messagee, Exception? exception = null)
        => ReportProgress(LogMessage.Log(logLevel, messagee, exception));

    protected void LogTrace(string message, Exception? exception = null)
        => ReportProgress(LogMessage.LogTrace(message, exception));

    protected void LogDebug(string message, Exception? exception = null)
        => ReportProgress(LogMessage.LogDebug(message, exception));

    protected void LogInformation(string message, Exception? exception = null)
        => ReportProgress(LogMessage.LogInformation(message, exception));

    protected void LogWarning(string message, Exception? exception = null)
        => ReportProgress(LogMessage.LogWarning(message, exception));

    protected void LogError(string message, Exception? exception = null)
        => ReportProgress(LogMessage.LogError(message, exception));

    protected void LogCritical(string message, Exception? exception = null)
        => ReportProgress(LogMessage.LogCritical(message, exception));

    #endregion

    #region ProgressReporting

    protected void SetProgress(string message, LogLevel logLevel = LogLevel.Debug)
    {
        ReportProgress(UpdateProgressBarStatus.Create(message));
        Log(logLevel, message);
    }

    #endregion

    #endregion

    #region UI Methods

    protected virtual void UpdateProgress(object? sender, ProgressChangedEventArgs e)
    {
        switch (e.UserState)
        {
            case string stateMessage: _messenger.Send(UpdateProgressBarStatus.Create(stateMessage)); break;
            case UpdateProgressBarStatus updateStatusMessage: _messenger.Send(updateStatusMessage); break;
            case UpdateProgressBarValue updateStatusValue: _messenger.Send(updateStatusValue); break;
            case LogMessage logMessage: SendLogMessage(logMessage); break;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Don't care")]
    private void SendLogMessage(LogMessage logMessage)
    {
        switch (logMessage.LogLevel)
        {
            case LogLevel.Trace:
                _logger.LogTrace(logMessage.Exception, logMessage.Message);
                break;
            case LogLevel.Debug:
                _logger.LogDebug(logMessage.Exception, logMessage.Message);
                break;
            case LogLevel.Information:
                _logger.LogInformation(logMessage.Exception, logMessage.Message);
                break;
            case LogLevel.Warning:
                _logger.LogWarning(logMessage.Exception, logMessage.Message);
                break;
            case LogLevel.Error:
                _logger.LogError(logMessage.Exception, logMessage.Message);
                break;
            case LogLevel.Critical:
                _logger.LogCritical(logMessage.Exception, logMessage.Message);
                break;
        }
    }

    #endregion
}
