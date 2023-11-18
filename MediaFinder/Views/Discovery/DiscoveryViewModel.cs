using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using MediaFinder.DataAccessLayer;
using MediaFinder.Helpers;
using MediaFinder.Logging;
using MediaFinder.Messages;
using MediaFinder.Models;
using MediaFinder.Services.Search;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaFinder.Views.Discovery;

[ObservableObject]
public partial class DiscoveryViewModel : ProgressableViewModel,
    IRecipient<SearchSettingUpdated>,
    IRecipient<WorkingDirectoryCreated>,
    IRecipient<FinishedMessage>,
    IRecipient<FileExtracted>
{
    private readonly ILogger<DiscoveryViewModel> _logger;
    private readonly SearchStageOneWorker _searchStageOneWorker;
    private readonly SearchStageTwoWorker _searchStagaeTwoWorker;
    private readonly SearchStageThreeWorker _searchStagaeThreeWorker;

    public DiscoveryViewModel(
        AddSearchSettingViewModel searchConfigViewModel,
        EditSearchSettingViewModel editSearchSettingViewModel,
        MediaFinderDbContext dbContext,
        IMessenger messenger,
        ILogger<DiscoveryViewModel> logger,
        SearchStageOneWorker searchStageOneWorker,
        SearchStageTwoWorker searchStageTwoWorker,
        SearchStageThreeWorker searchStageThreeWorker)
        : base(messenger, logger, dbContext)
    {
        _logger = logger;
        _searchStageOneWorker = searchStageOneWorker;
        _searchStagaeTwoWorker = searchStageTwoWorker;
        _searchStagaeThreeWorker = searchStageThreeWorker;

        messenger.RegisterAll(this);

        BindingOperations.EnableCollectionSynchronization(Configurations, new());

        _searchStageOneWorker.RunWorkerCompleted += SearchStepOneCompleted;
        _searchStagaeTwoWorker.RunWorkerCompleted += SearchStageTwoCompleted;
        _searchStagaeThreeWorker.RunWorkerCompleted += SearchStageThreeCompleted;

        SearchConfigViewModel = searchConfigViewModel;
        EditSearchConfigViewModel = editSearchSettingViewModel;
        WorkingDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TEMP");
    }

    #region Settings Configurations

    [ObservableProperty]
    private bool _drawEntityIsNew;

    public AddSearchSettingViewModel SearchConfigViewModel { get; set; }

    public EditSearchSettingViewModel EditSearchConfigViewModel { get; set; }

    [RelayCommand]
    private void OnAddSearchSetting(DrawerHost drawerHost)
    {
        DrawEntityIsNew = true;
        drawerHost!.IsRightDrawerOpen = true;
    }

    [RelayCommand]
#pragma warning disable CRR0034
#pragma warning disable CRR0035
    private async Task OnEditSearchSetting(DrawerHost drawerHost)
#pragma warning restore CRR0035
#pragma warning restore CRR0034
    {
        DrawEntityIsNew = false;
#pragma warning disable CRR0039
        await EditSearchConfigViewModel.InitializeAsync(SelectedConfig!.Id).ConfigureAwait(true);
#pragma warning restore CRR0039
        drawerHost!.IsRightDrawerOpen = true;
    }

    private bool CanRemoveSearchSetting()
        => SelectedConfig is not null;

    [RelayCommand(CanExecute = nameof(CanRemoveSearchSetting))]
#pragma warning disable CRR0034
#pragma warning disable CRR0035
    private async Task OnRemoveSearchSetting()
#pragma warning restore CRR0035
#pragma warning restore CRR0034
    {
        var config = SelectedConfig!;
#pragma warning disable CRR0039
        var entity = await _dbContext.SearchSettings
            .FirstOrDefaultAsync(x => x.Id == config.Id)
            .ConfigureAwait(true);
#pragma warning restore CRR0039
        if (entity is null)
        {
            return;
        }

        _dbContext.SearchSettings.Remove(entity);
#pragma warning disable CRR0039
        await _dbContext.SaveChangesAsync().ConfigureAwait(true);
#pragma warning restore CRR0039
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
    private ObservableCollection<SearchConfiguration> _configurations = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private string? _workingDirectory;

    [ObservableProperty]
    private ulong _workingDirectorySize;

    private string? _createdWorkingDirectory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveSearchSettingCommand))]
    private SearchConfiguration? _selectedConfig;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(FinishSearchCommand))]
    private bool _searchComplete;

    partial void OnWorkingDirectoryChanged(string? value)
    {
        CleanupWorkingDirectory();
        SearchComplete = false;
    }

    partial void OnSelectedConfigChanged(SearchConfiguration? value)
    {
        CleanupWorkingDirectory();
        SearchComplete = false;
    }

    partial void OnSearchCompleteChanged(bool oldValue, bool newValue)
    {
        if (newValue is true && newValue != oldValue)
        {
            _messenger.Send(SearchCompletedMessage.Create());
            _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
        }
    }

    public void Receive(WorkingDirectoryCreated message)
    {
        _createdWorkingDirectory = message.Directory;
    }

    public void Receive(FileExtracted message)
    {
        if (WorkingDirectory is null)
        {
            WorkingDirectorySize = 0UL;
            return;
        }

        var workingDir = new DirectoryInfo(WorkingDirectory!);
        if (!workingDir.Exists)
        {
            WorkingDirectorySize = 0UL;
            return;
        }

        WorkingDirectorySize = workingDir.GetDirectorySize();
    }

    [RelayCommand]
#pragma warning disable CRR0034
#pragma warning disable CRR0035
    public async Task LoadConfigurations()
#pragma warning restore CRR0035
#pragma warning restore CRR0034
    {
        ShowProgressIndicator("Loading...");

        Configurations.Clear();
        await foreach (var config in _dbContext.SearchSettings
            .Include(ss => ss.Directories)
            .AsAsyncEnumerable())
        {
            Configurations.Add(SearchConfiguration.Create(config));
        }

        HideProgressIndicator();
    }

    public bool CanPerformSearch()
        => !string.IsNullOrEmpty(WorkingDirectory)
            && SelectedConfig is not null
            && !SearchComplete
            && !_searchStageOneWorker.IsBusy
            && !_searchStagaeTwoWorker.IsBusy
            && !_searchStagaeThreeWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanPerformSearch))]
#pragma warning disable CRR0034
#pragma warning disable CRR0035
    public async Task OnPerformSearch()
#pragma warning restore CRR0035
#pragma warning restore CRR0034
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

        if (!_searchStageOneWorker.IsBusy && !_searchStagaeTwoWorker.IsBusy && !_searchStagaeThreeWorker.IsBusy)
        {
            ShowProgressIndicator("Initializing search parameters", CancelSearchCommand);

#pragma warning disable CRR0039
            await TruncateFileDetailStateAsync(_dbContext).ConfigureAwait(true);
#pragma warning restore CRR0039

            _searchStageOneWorker.RunWorkerAsync(SearchRequest.Create(_progressToken, WorkingDirectory!, SelectedConfig!));
            CancelSearchCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanCancelSearch()
        => ((_searchStageOneWorker.IsBusy && !_searchStageOneWorker.CancellationPending)
            || (_searchStagaeTwoWorker.IsBusy && !_searchStagaeTwoWorker.CancellationPending)
            || (_searchStagaeThreeWorker.IsBusy && !_searchStagaeThreeWorker.CancellationPending));

    [RelayCommand(CanExecute = nameof(CanCancelSearch))]
    private void OnCancelSearch()
    {
        if (!_searchStageOneWorker.IsBusy
            && !_searchStagaeTwoWorker.IsBusy
            && !_searchStagaeThreeWorker.IsBusy)
        {
            return;
        }

        CancelProgressIndicator("Cancelling...");
        if (_searchStageOneWorker.IsBusy)
        {
            _searchStageOneWorker.CancelAsync();
        }
        if (_searchStagaeTwoWorker.IsBusy)
        {
            _searchStagaeTwoWorker.CancelAsync();
        }
        if (_searchStagaeThreeWorker.IsBusy)
        {
            _searchStagaeThreeWorker.CancelAsync();
        }
        CancelSearchCommand.NotifyCanExecuteChanged();
    }

    private void SearchStepOneCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            _messenger.Send(SnackBarMessage.Create("Search cancelled"));
            _logger.UserCanceledOperation("Media Discovery");
            SearchCleanup();
            return;
        }
        if (e.Error is not null)
        {
            _messenger.Send(SnackBarMessage.Create($"Search failed: {e.Error.Message}"));
            _logger.ProcessFailed(e.Error);
            SearchCleanup();
            return;
        }
        if (e.Result is not SearchResponse result)
        {
            _messenger.Send(SnackBarMessage.Create("Search returned invalid result"));
            _logger.InvalidResult("SearchWorker1", e.Result!.GetType());
            SearchCleanup();
            return;
        }

        _searchStagaeTwoWorker.RunWorkerAsync(
            AnalyseRequest.Create(
                _progressToken,
                result.Files,
                SelectedConfig!.Directories,
                WorkingDirectory!,
                SelectedConfig.PerformDeepAnalysis));
    }

    private void SearchStageTwoCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            _messenger.Send(SnackBarMessage.Create("Search cancelled"));
            _logger.UserCanceledOperation("File Analysis");
            SearchCleanup();
            return;
        }
        if (e.Error is not null)
        {
            _messenger.Send(SnackBarMessage.Create($"Search failed"));
            _logger.ProcessFailed(e.Error);
            SearchCleanup();
            return;
        }
        if (e.Result is not AnalysisResponse result)
        {
            _messenger.Send(SnackBarMessage.Create("Search returned invalid result"));
            _logger.InvalidResult("SearchWorker2", e.Result!.GetType());
            SearchCleanup();
            return;
        }

        try
        {
            _dbContext.FileDetails.AddRange(result.Files);
            _dbContext.SaveChanges();
        }
        catch (DbException ex)
        {
            _messenger.Send(SnackBarMessage.Create("Failed to save search results"));
            _logger.DatabaseError(ex, "saving initial search results");
            SearchCleanup();
            return;
        }

        _searchStagaeThreeWorker.RunWorkerAsync(
            FilterRequest.Create(
                _progressToken,
                SelectedConfig!.MinImageWidth,
                SelectedConfig.MinImageHeight,
                SelectedConfig.MinVideoWidth,
                SelectedConfig.MinVideoHeight));
    }

    private void SearchStageThreeCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            _messenger.Send(SnackBarMessage.Create("Discovery process cancelled"));
            _logger.UserCanceledOperation("Filtering");
            SearchCleanup();
            return;
        }
        if (e.Error is not null)
        {
            _messenger.Send(SnackBarMessage.Create("Discovery failed"));
            _logger.ProcessFailed(e.Error);
            SearchCleanup();
            return;
        }
        if (e.Result is not bool result || !result)
        {
            _messenger.Send(SnackBarMessage.Create("Discovery process returned invalid analysis result"));
            _logger.InvalidResult("SearchWorker3", e.Result!.GetType());
            SearchCleanup();
            return;
        }

        _messenger.Send(SnackBarMessage.Create("Discovery process completed"));
        SearchFinished();
    }

    private void SearchCleanup()
    {
        CleanupWorkingDirectory();
        HideProgressIndicator();
        CancelSearchCommand.NotifyCanExecuteChanged();
        PerformSearchCommand.NotifyCanExecuteChanged();
    }

    private void SearchFinished()
    {
        HideProgressIndicator();
        SearchComplete = true;
    }

    private bool CanMoveToReview()
        => SearchComplete
            && !_searchStageOneWorker.IsBusy
            && !_searchStagaeTwoWorker.IsBusy
            && !_searchStagaeThreeWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanMoveToReview))]
    private void OnMoveToReview()
    {
        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
    }

    #endregion

    #region Finish Actions

    private void CleanupWorkingDirectory()
    {
        if (!string.IsNullOrEmpty(_createdWorkingDirectory)
            && Directory.Exists(_createdWorkingDirectory))
        {
            Directory.Delete(_createdWorkingDirectory, true);
            _logger.RemovedWorkingDirectory(_createdWorkingDirectory);
            _createdWorkingDirectory = null;
            WorkingDirectorySize = 0UL;
        }
    }

    public bool CanFinishSearch()
        => (_createdWorkingDirectory is not null || _dbContext.FileDetails.Any())
            && !_searchStageOneWorker.IsBusy
            && !_searchStagaeTwoWorker.IsBusy
            && !_searchStagaeThreeWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanFinishSearch))]
    public void OnFinishSearch()
    {
        _messenger.Send(FinishedMessage.Create());
    }

    public async void Receive(FinishedMessage message)
    {
        try
        {
#pragma warning disable CRR0039
            await CleanupAsync().ConfigureAwait(true);
#pragma warning restore CRR0039
        }
        catch
        {
            // do nothing
        }
    }

    internal async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        _logger.SessionCleanup();

        await TruncateFileDetailStateAsync(_dbContext, cancellationToken).ConfigureAwait(true);

        CleanupWorkingDirectory();

        if (SearchComplete)
        {
            SearchComplete = false;
            SelectedConfig = null;
        }
    }

    #endregion
}
