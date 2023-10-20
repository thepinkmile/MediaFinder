﻿using System.ComponentModel;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.Messages;

using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Helpers;

public abstract class ReactiveBackgroundWorker<T> : BackgroundWorker
    where T : ReactiveBackgroundWorkerContextBase
{
    private readonly ILogger _logger;
    private readonly IMessenger _messenger;

    private object _progressToken = null!;

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

    #region Logging

    protected void Log(LogLevel logLevel, Exception exception, string messagee, params string[] args)
        => ReportProgress(LogMessage.Log(logLevel, exception, messagee, args));

    protected void Log(LogLevel logLevel, string messagee, params string[] args)
        => ReportProgress(LogMessage.Log(logLevel, messagee, args));

    protected void LogTrace(Exception exception, string messagee, params string[] args)
        => ReportProgress(LogMessage.LogTrace(exception, messagee, args));

    protected void LogTrace(string messagee, params string[] args)
        => ReportProgress(LogMessage.LogTrace(messagee, args));

    protected void LogDebug(Exception exception, string messagee, params string[] args)
        => ReportProgress(LogMessage.LogDebug(exception, messagee, args));

    protected void LogDebug(string messagee, params string[] args)
        => ReportProgress(LogMessage.LogDebug(messagee, args));

    protected void LogInformation(Exception exception, string messagee, params string[] args)
        => ReportProgress(LogMessage.LogInformation(exception, messagee, args));

    protected void LogInformation(string messagee, params string[] args)
        => ReportProgress(LogMessage.LogInformation(messagee, args));

    protected void LogWarning(Exception exception, string messagee, params string[] args)
        => ReportProgress(LogMessage.LogWarning(exception, messagee, args));

    protected void LogWarning(string messagee, params string[] args)
        => ReportProgress(LogMessage.LogWarning(messagee, args));

    protected void LogError(Exception exception, string messagee, params string[] args)
        => ReportProgress(LogMessage.LogError(exception, messagee, args));

    protected void LogError(string messagee, params string[] args)
        => ReportProgress(LogMessage.LogError(messagee, args));

    protected void LogCritical(Exception exception, string messagee, params string[] args)
        => ReportProgress(LogMessage.LogCritical(exception, messagee, args));

    protected void LogCritical(string messagee, params string[] args)
        => ReportProgress(LogMessage.LogCritical(messagee, args));

    #endregion

    #region ProgressReporting

    protected void ReportProgress(object userState)
        => ReportProgress(default, userState);

    protected void SetProgress(string message, LogLevel logLevel = LogLevel.Debug)
    {
        ReportProgress(UpdateProgressMessage.Create(_progressToken, message));
        Log(logLevel, message);
    }

    #endregion

    #endregion

    #region UI Methods

    protected virtual void UpdateProgress(object? sender, ProgressChangedEventArgs e)
    {
        switch (e.UserState)
        {
            case string stateMessage: _messenger.Send(UpdateProgressMessage.Create(_progressToken, stateMessage)); break;
            case UpdateProgressMessage updateStatusMessage: _messenger.Send(updateStatusMessage); break;
            case LogMessage logMessage: SendLogMessage(logMessage); break;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "It is, but it is passed from an underlaying process/thread")]
    private void SendLogMessage(LogMessage logMessage)
        => _logger.Log(logMessage.LogLevel, logMessage.Exception, logMessage.Message, logMessage.FormatArgs);

    #endregion
}
