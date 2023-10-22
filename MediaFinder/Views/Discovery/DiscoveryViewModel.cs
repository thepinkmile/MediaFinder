using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Helpers;
using MediaFinder_v2.Messages;
using MediaFinder_v2.Services.Search;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Views.Discovery;

[ObservableObject]
public partial class DiscoveryViewModel : ProgressableViewModel,
    IRecipient<SearchSettingUpdated>,
    IRecipient<WorkingDirectoryCreated>,
    IRecipient<FinishedMessage>
{
    private readonly ILogger<DiscoveryViewModel> _logger;
    private readonly SearchStageOneWorker _searchStageOneWorker;
    private readonly SearchStageTwoWorker _searchStagaeTwoWorker;
    private readonly SearchStageThreeWorker _searchStagaeThreeWorker;

    public DiscoveryViewModel(
        AddSearchSettingViewModel searchConfigViewModel,
        AppDbContext dbContext,
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
        WorkingDirectory = Path.GetTempPath();
    }

    #region Settings Configurations

    public AddSearchSettingViewModel SearchConfigViewModel { get; set; }

    [RelayCommand]
    private static void OnAddSearchSetting(DrawerHost drawerHost)
    {
        drawerHost!.IsRightDrawerOpen = true;
    }

    private bool CanRemoveSearchSetting()
        => SelectedConfig is not null;

    [RelayCommand(CanExecute = nameof(CanRemoveSearchSetting))]
    private async Task OnRemoveSearchSetting()
    {
        var config = SelectedConfig!;
        var entity = await _dbContext.SearchSettings.FirstOrDefaultAsync(x => x.Id == config.Id);
        if (entity is null)
        {
            return;
        }

        _dbContext.SearchSettings.Remove(entity);
        await _dbContext.SaveChangesAsync();
        _messenger.Send(SearchSettingUpdated.Create(config));
        _messenger.Send(SnackBarMessage.Create($"Removed configuration: {config.Name}"));
    }

    public async void Receive(SearchSettingUpdated message)
    {
        if (LoadConfigurationsCommand.IsRunning)
            return;

        await LoadConfigurationsCommand.ExecuteAsync(null);
    }

    #endregion

    #region Discovery Process

    [ObservableProperty]
    private ObservableCollection<SearchConfiguration> _configurations = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private string? _workingDirectory;

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

    [RelayCommand]
    public async Task LoadConfigurations()
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
    public async Task OnPerformSearch()
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

            await TruncateFileDetailState(_dbContext);

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
            _logger.LogInformation("Process cancelled by user.");
            SearchCleanup();
            return;
        }
        if (e.Error is not null)
        {
            _messenger.Send(SnackBarMessage.Create($"Search failed: {e.Error.Message}"));
            _logger.LogError(e.Error, "Process Failed.");
            SearchCleanup();
            return;
        }
        if (e.Result is not SearchResponse result)
        {
            _messenger.Send(SnackBarMessage.Create($"Search returned invalid result: {e.Result!.GetType()}"));
            _logger.LogError("Invalid SearchWorker Result: {resultType}", e.Result.GetType());
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
            _logger.LogInformation("Process cancelled by user.");
            SearchCleanup();
            return;
        }
        if (e.Error is not null)
        {
            _messenger.Send(SnackBarMessage.Create($"Search failed: {e.Error.Message}"));
            _logger.LogError(e.Error, "Process Failed.");
            SearchCleanup();
            return;
        }
        if (e.Result is not AnalysisResponse result)
        {
            _messenger.Send(SnackBarMessage.Create($"Search returned invalid result: {e.Result!.GetType()}"));
            _logger.LogError("Invalid SearchWorker Result: {resultType}", e.Result.GetType());
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
            _messenger.Send(SnackBarMessage.Create($"Failed to save search results"));
            _logger.LogError(ex, "Failed to save initial search results");
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
            _logger.LogInformation("Process cancelled by user.");
            SearchCleanup();
            return;
        }
        if (e.Error is not null)
        {
            _messenger.Send(SnackBarMessage.Create($"Discovery failed: {e.Error.Message}"));
            _logger.LogError(e.Error, "Process Failed.");
            SearchCleanup();
            return;
        }
        if (e.Result is not bool result || !result)
        {
            _messenger.Send(SnackBarMessage.Create($"Discovery process returned invalid analysis result"));
            _logger.LogError("Invalid SearchWorker Result: Stage 3 returned false");
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
            _logger.LogDebug("Removed Working Directory: {WorkingDirectory}", _createdWorkingDirectory);
            _createdWorkingDirectory = null;
        }
    }

    public bool CanFinishSearach()
        => (_createdWorkingDirectory is not null || _dbContext.FileDetails.Any())
            && !_searchStageOneWorker.IsBusy
            && !_searchStagaeTwoWorker.IsBusy
            && !_searchStagaeThreeWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanFinishSearach))]
    public void OnFinishSearch()
    {
        _messenger.Send(FinishedMessage.Create());
    }

    public async void Receive(FinishedMessage message)
    {
        await Cleanup();
    }

    internal async Task Cleanup()
    {
        _logger.LogInformation("Cleaning up search session.");
        await TruncateFileDetailState(_dbContext);

        CleanupWorkingDirectory();

        if (SearchComplete)
        {
            SearchComplete = false;
            SelectedConfig = null;
        }
    }

    #endregion
}
