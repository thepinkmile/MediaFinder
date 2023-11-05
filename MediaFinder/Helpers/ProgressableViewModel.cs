using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Logging;
using MediaFinder_v2.Messages;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Windows.Input;

namespace MediaFinder_v2.Helpers;

#pragma warning disable CRRSP08
public abstract class ProgressableViewModel
#pragma warning restore CRRSP08
{
    private readonly ILogger _logger;

#pragma warning disable CRRSP11
    protected ProgressableViewModel(
#pragma warning restore CRRSP11
        IMessenger messenger,
        ILogger logger,
        AppDbContext dbContext)
    {
        _progressToken = Guid.NewGuid();
        _messenger = messenger;
        _logger = logger;
        _dbContext = dbContext;
    }

    protected readonly IMessenger _messenger;
    protected readonly AppDbContext _dbContext;
    protected readonly object _progressToken;

    #region Progress Actions
#pragma warning disable CA2254 // Template should be a static expression

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

#pragma warning restore CA2254 // Template should be a static expression
    #endregion

    #region Completion Actions

    protected static async Task TruncateFileDetailStateAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.FileDetails.Include(fd => fd.FileProperties).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(true);
        dbContext.ChangeTracker.Clear();
    }

    #endregion
}
