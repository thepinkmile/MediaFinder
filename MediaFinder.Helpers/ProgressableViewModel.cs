using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using MediaFinder.DataAccessLayer;
using MediaFinder.Logging;
using MediaFinder.Messages;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Windows.Input;

namespace MediaFinder.Helpers;

public abstract class ProgressableViewModel : ObservableObject
{
    private readonly ILogger _logger;

    protected ProgressableViewModel(
        IMessenger messenger,
        ILogger logger,
        MediaFinderDbContext dbContext)
    {
        _progressToken = Guid.NewGuid();
        _messenger = messenger;
        _logger = logger;
        _dbContext = dbContext;
    }

    protected readonly IMessenger _messenger;
    protected readonly MediaFinderDbContext _dbContext;
    protected readonly object _progressToken;

    #region Progress Actions

    protected void ShowProgressIndicator(string message, ICommand? cancelCommand = null)
    {
        _messenger.Send(ShowProgressMessage.Create(_progressToken, message, cancelCommand));
        _logger.ProgressUpdate(message);
    }

    protected void UpdateProgressIndicator(string message)
    {
        _messenger.Send(UpdateProgressMessage.Create(_progressToken, message));
        _logger.ProgressUpdate(message);
    }

    protected void CancelProgressIndicator(string message)
    {
        _messenger.Send(CancelProgressMessage.Create(_progressToken));
        _logger.ProgressUpdate(message);
    }

    protected void HideProgressIndicator()
    {
        _messenger.Send(CompleteProgressMessage.Create(_progressToken));
        _logger.ProgressUpdate("Process Complete.");
    }

    #endregion

    #region Completion Actions

    protected static async Task TruncateFileDetailStateAsync(MediaFinderDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.FileDetails.Include(fd => fd.FileProperties).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(true);
        dbContext.ChangeTracker.Clear();
    }

    #endregion
}
