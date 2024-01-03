using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MediaFinder.DataAccessLayer;
using MediaFinder.DiscoveryServices;
using MediaFinder.Helpers;
using MediaFinder.Logging;
using MediaFinder.Messages;
using MediaFinder.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediaFinder.Views.Discovery;

public partial class DiscoveryViewModel : ProgressableViewModel,
    IRecipient<SearchSettingUpdated>,
    IRecipient<FinishedMessage>,
    IRecipient<FileExtracted>,
    IRecipient<DiscoveryCompletedMessage>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DiscoveryViewModel> _logger;
    private readonly Progress<object> _progressReporter;

    public DiscoveryViewModel(
        IServiceProvider serviceProvider,
        MediaFinderDbContext dbContext,
        IMessenger messenger,
        ILogger<DiscoveryViewModel> logger)
        : base(messenger, logger, dbContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        messenger.RegisterAll(this);

        BindingOperations.EnableCollectionSynchronization(Configurations, new());

        _progressReporter = new();
        _progressReporter.ProgressChanged += TmpProgressReporter_ProgressChanged;

        // TODO: Should this default to temp directory???
        WorkingDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TEMP");
    }

    private void TmpProgressReporter_ProgressChanged(object? sender, object e)
    {
        switch (e)
        {
            case string statusMessage: _messenger.Send(UpdateProgressMessage.Create(_progressToken!, statusMessage)); break;
            case WorkingDirectoryCreated wdcMessage: _messenger.Send(wdcMessage); break;
            case FileExtracted feMessage: _messenger.Send(feMessage); break;
            case SnackBarMessage sbMessage: _messenger.Send(sbMessage); break;
            case DiscoveryCompletedMessage dcMessage: _messenger.Send(dcMessage); break;
        }
    }

    #region Settings Configurations

    [ObservableProperty]
    private UserControl? _drawContent;

    [ObservableProperty]
    private bool _editorVisible;

    [RelayCommand]
    private void OnAddSearchSetting()
    {
        DrawContent = _serviceProvider.GetRequiredService<AddSearchSettingView>();
        EditorVisible = true;
    }

    [RelayCommand]
    private async Task OnEditSearchSetting()
    {
        var view = _serviceProvider.GetRequiredService<EditSearchSettingView>();
        await view.InitializeDataContextAsync(SelectedConfig!.Id).ConfigureAwait(true);
        DrawContent = view;
        EditorVisible = true;
    }

    private bool CanRemoveSearchSetting()
        => SelectedConfig is not null;

    [RelayCommand(CanExecute = nameof(CanRemoveSearchSetting))]
    private async Task OnRemoveSearchSetting()
    {
        var config = SelectedConfig!;
        var entity = await _dbContext.SearchSettings
            .FirstOrDefaultAsync(x => x.Id == config.Id)
            .ConfigureAwait(true);
        if (entity is null)
        {
            return;
        }

        _dbContext.SearchSettings.Remove(entity);
        await _dbContext.SaveChangesAsync().ConfigureAwait(true);
        _messenger.Send(SearchSettingUpdated.Create(config));
        _messenger.Send(SnackBarMessage.Create($"Removed configuration: {config.Name}"));
    }

    public async void Receive(SearchSettingUpdated message)
    {
        try
        {
            if (LoadConfigurationsCommand.IsRunning)
                return;

            await LoadConfigurationsCommand.ExecuteAsync(null).ConfigureAwait(false);
        }
        catch
        {
            // do nothing
        }
    }

    #endregion

    #region Discovery Process

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(FinishSearchCommand))]
    private Task? _discoveryTask;

    private CancellationTokenSource? _discoveryTaskCancellationSource;

    [ObservableProperty]
    private ObservableCollection<DiscoveryOptions> _configurations = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private string? _workingDirectory;

    [ObservableProperty]
    private ulong _workingDirectorySize;

    private Guid? _currentRunIdewntifier;

    private async ValueTask<DataAccessLayer.Models.DiscoveryExecution?> GetCurrentRunDetails()
        => await _dbContext.Runs.FindAsync([_currentRunIdewntifier]);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveSearchSettingCommand))]
    private DiscoveryOptions? _selectedConfig;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(FinishSearchCommand))]
    private bool _searchComplete;

    partial void OnWorkingDirectoryChanged(string? value)
    {
        _messenger.Send(FinishedMessage.Create());
    }

    partial void OnSelectedConfigChanged(DiscoveryOptions? value)
    {
        _messenger.Send(FinishedMessage.Create());
    }

    public async void Receive(FileExtracted message)
    {
        var runDetails = await GetCurrentRunDetails();
        if (runDetails is not { })
        {
            return;
        }

        if (string.IsNullOrEmpty(runDetails.WorkingDirectory))
        {
            WorkingDirectorySize = 0UL;
            return;
        }

        var workingDir = new DirectoryInfo(runDetails.WorkingDirectory);
        if (!workingDir.Exists)
        {
            WorkingDirectorySize = 0UL;
            return;
        }

        WorkingDirectorySize = workingDir.GetDirectorySize();
    }

    [RelayCommand]
    public async Task LoadConfigurations()
    {
        ShowProgressIndicator("Loading...");

        Configurations.Clear();
        await foreach (var config in _dbContext.SearchSettings
            .Include(ss => ss.Directories)
            .AsAsyncEnumerable())
        {
            Configurations.Add(config.ToDiscoveryOptions());
        }

        HideProgressIndicator();
    }

    public bool CanPerformSearch()
        => !string.IsNullOrEmpty(WorkingDirectory)
            && SelectedConfig is not null
            && !SearchComplete
            && (DiscoveryTask is null || DiscoveryTask.IsCompleted);

    [RelayCommand(CanExecute = nameof(CanPerformSearch))]
    public void OnPerformSearch()
    {
        if (SelectedConfig is null)
        {
            _messenger.Send(SnackBarMessage.Create("No configuration selected"));
            return;
        }

        if (WorkingDirectory is null)
        {
            _messenger.Send(SnackBarMessage.Create("No working directory selected"));
            return;
        }

        if (DiscoveryTask is null || DiscoveryTask.IsCompleted)
        {
            _discoveryTaskCancellationSource = new();
            DiscoveryTask = Task.Run(() => DoDiscovery(_discoveryTaskCancellationSource.Token));
        }
    }

    private bool CanCancelSearch()
        => DiscoveryTask is not null && !DiscoveryTask.IsCompleted
            && _discoveryTaskCancellationSource is not null && !_discoveryTaskCancellationSource.IsCancellationRequested;

    [RelayCommand(CanExecute = nameof(CanCancelSearch))]
    private void OnCancelSearch()
    {
        CancelProgressIndicator("Cancelling...");
        _discoveryTaskCancellationSource!.Cancel();
        CancelSearchCommand.NotifyCanExecuteChanged();
    }

    private async Task DoDiscovery(CancellationToken cancellationToken = default)
    {
        ShowProgressIndicator("Initializing search parameters", CancelSearchCommand);
        IProgress<object> progressReporter = _progressReporter;

        try
        {
            // TODO: update DiscoveryRunnerService to create new Db Tennant for run.
            await TruncateFileDetailStateAsync(_dbContext, cancellationToken).ConfigureAwait(true);
            cancellationToken.ThrowIfCancellationRequested();

            _currentRunIdewntifier = await _serviceProvider
                .GetRequiredService<DiscoveryRunnerService>()
                .CreateRunContext(WorkingDirectory!, SelectedConfig!, progressReporter, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: Add next steps
            cancellationToken.ThrowIfCancellationRequested();

            SearchComplete = true;
            progressReporter.Report(SnackBarMessage.Create("Discovery process completed"));
            progressReporter.Report(DiscoveryCompletedMessage.Create());
        }
        catch (TaskCanceledException ex)
        {
            progressReporter.Report(SnackBarMessage.Create("Search cancelled"));
            _logger.UserCanceledOperation(ex.Source ?? "DiscoveryViewModel::DoDiscovery");
            progressReporter.Report(FinishedMessage.Create());
        }
        catch(Exception ex)
        {
            progressReporter.Report(SnackBarMessage.Create($"Search failed: {ex.Message}"));
            _logger.ProcessFailed(ex);
            progressReporter.Report(FinishedMessage.Create());
        }
        finally
        {
            _discoveryTaskCancellationSource = null;
            DiscoveryTask = null;
            HideProgressIndicator();
        }
    }

    public void Receive(DiscoveryCompletedMessage message)
    {
        if (CanMoveToReview())
        {
            MoveToReviewCommand.Execute(null);
        }
    }

    private bool CanMoveToReview()
        => SearchComplete
            && (DiscoveryTask is null || DiscoveryTask.IsCompleted);

    [RelayCommand(CanExecute = nameof(CanMoveToReview))]
    private void OnMoveToReview()
    {
        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
    }

    #endregion

    #region Finish Actions

    private async Task CleanupWorkingDirectory()
    {
        var runDetails = await GetCurrentRunDetails();
        if (runDetails is not { })
        {
            return;
        }

        if (Directory.Exists(runDetails.WorkingDirectory))
        {
            Directory.Delete(runDetails.WorkingDirectory, true);
            _logger.RemovedWorkingDirectory(runDetails.WorkingDirectory);
            WorkingDirectorySize = 0UL;
        }
    }

    public bool CanFinishSearch()
        => _currentRunIdewntifier is not null
            && (DiscoveryTask is null || DiscoveryTask.IsCompleted);

    [RelayCommand(CanExecute = nameof(CanFinishSearch))]
    public void OnFinishSearch()
    {
        _messenger.Send(FinishedMessage.Create());
    }

    public async void Receive(FinishedMessage message)
    {
        await CleanupAsync().ConfigureAwait(true);
    }

    internal async Task CleanupAsync()
    {
        try
        {
            _logger.SessionCleanup();

            await TruncateFileDetailStateAsync(_dbContext).ConfigureAwait(true);

            await CleanupWorkingDirectory();

            var runDetails = await GetCurrentRunDetails();
            if (runDetails is { })
            {
                _dbContext.Runs.Remove(runDetails);
                await _dbContext.SaveChangesAsync();
            }

            if (SearchComplete)
            {
                SearchComplete = false;
                SelectedConfig = null;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception Occured.");
        }
    }

    #endregion
}
