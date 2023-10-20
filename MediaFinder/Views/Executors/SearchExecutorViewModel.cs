using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Windows.Data;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Messages;
using MediaFinder_v2.Services.Export;
using MediaFinder_v2.Services.Search;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Views.Executors;

#pragma warning disable CA2254 // Template should be a static expression - Don't care about formats here
public partial class SearchExecutorViewModel : ObservableObject,
    IRecipient<WorkingDirectoryCreated>,
    IRecipient<SearchSettingUpdated>,
    IRecipient<WizardNavigationMessage>,
    IRecipient<SearchCompletedMessage>
{
    private readonly IServiceProvider _serviceProvider;

    private readonly AppDbContext _dbContext;
    private readonly IMessenger _messenger;
    private readonly ILogger<SearchExecutorViewModel> _logger;

    private readonly SearchStageOneWorker _searchStageOneWorker;
    private readonly SearchStageTwoWorker _searchStagaeTwoWorker;
    private readonly SearchStageThreeWorker _searchStagaeThreeWorker;
    private readonly ExportWorker _exportWorker;

    public SearchExecutorViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _messenger = _serviceProvider.GetRequiredService<IMessenger>();
        _logger = _serviceProvider.GetRequiredService<ILogger<SearchExecutorViewModel>>();
        SearchConfigViewModel = _serviceProvider.GetRequiredService<AddSearchSettingViewModel>();
        _searchStageOneWorker = _serviceProvider.GetRequiredService<SearchStageOneWorker>();
        _searchStagaeTwoWorker = _serviceProvider.GetRequiredService<SearchStageTwoWorker>();
        _searchStagaeThreeWorker = _serviceProvider.GetRequiredService<SearchStageThreeWorker>();
        _exportWorker = _serviceProvider.GetRequiredService<ExportWorker>();

        _messenger.RegisterAll(this);

        BindingOperations.EnableCollectionSynchronization(Configurations, new());
        BindingOperations.EnableCollectionSynchronization(DiscoveredFiles, new());
        

        // Bind CollectionViewSource.Source to MyCollection
        _mediaFilesViewSource = new CollectionViewSource();
        Binding myBind = new() { Source = DiscoveredFiles };
        BindingOperations.SetBinding(_mediaFilesViewSource, CollectionViewSource.SourceProperty, myBind);
        MediaFilesView = _mediaFilesViewSource.View;
        MediaFilesView.Filter = MediaFile_Filter_ShouldExport;
        FilterByShouldExport = true;

        _searchStageOneWorker.RunWorkerCompleted += SearchStepOneCompleted;
        _searchStagaeTwoWorker.RunWorkerCompleted += SearchStageTwoCompleted;
        _searchStagaeThreeWorker.RunWorkerCompleted += SearchStageThreeCompleted;
        _exportWorker.RunWorkerCompleted += ExportCompleted;
    }

    private void ShowProgressIndicator(string message)
    {
        _messenger.Send(ShowProgressBar.Create(message));
        _logger.LogInformation(message);
    }

    private void UpdateProgressIndicator(string message)
    {
        _messenger.Send(UpdateProgressBarStatus.Create(message));
        _logger.LogInformation(message);
    }

    private void HideProgressIndicator()
    {
        _messenger.Send(HideProgressBar.Create());
        _logger.LogInformation("Process Complete.");
    }

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

    #region ProgressOverlay

    [RelayCommand]
    private void Test()
    {
        ProgressCancelCommand = CancelProgressCommand;
        ProgressMessage = "Testing";
        ShowProgress = true;
        IsCancelling = false;
    }

    [ObservableProperty]
    private ICommand? _progressCancelCommand;

    [RelayCommand(CanExecute = nameof(CanCancelProgress))]
    private void OnCancelProgress()
    {
        ProgressMessage = "Cancelling...";
        IsCancelling = true;
        _ = PendingCancel();
        ProgressCancelCommand = null;
    }

    private async Task PendingCancel()
    {
        await Task.Delay(10_000);
        ShowProgress = false;
    }

    private bool CanCancelProgress
        => ShowProgress && !IsCancelling;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelProgressCommand))]
    private bool _isCancelling;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelProgressCommand))]
    private bool _showProgress;

    [ObservableProperty]
    private string? _progressMessage;

    [ObservableProperty]
    private int _progressValue;

    #endregion

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

    #region Step1 - Set Working Directory

    [ObservableProperty]
    private ObservableCollection<SearchConfiguration> _configurations = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private string? _workingDirectory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveSearchSettingCommand))]
    private SearchConfiguration? _selectedConfig;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(BackToSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSearchCommand))]
    private bool _searchComplete;

    partial void OnWorkingDirectoryChanged(string? value)
    {
        SearchComplete = false;
    }

    partial void OnSelectedConfigChanged(SearchConfiguration? value)
    {
        SearchComplete = false;
    }

    public void Receive(WorkingDirectoryCreated message)
    {
        SelectedConfig!.WorkingDirectory = message.Path;
        FinishCommand.NotifyCanExecuteChanged();
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
            ShowProgressIndicator("Initializing search parameters");

            await TruncateFileDetailState(_dbContext);

            _searchStageOneWorker.RunWorkerAsync(SearchRequest.Create(WorkingDirectory!, SelectedConfig!));
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
        _messenger.Send(UpdateProgressBarStatus.Create("Cancelling..."));
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
                result.Files,
                SelectedConfig!.Directories,
                SelectedConfig.WorkingDirectory!,
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
                SelectedConfig!.MinImageWidth,
                SelectedConfig.MinImageHeight,
                SelectedConfig.MinVideoWidth,
                SelectedConfig.MinVideoHeight));
    }

    private void SearchStageThreeCompleted(object? sender, RunWorkerCompletedEventArgs e)
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
        if (e.Result is not bool result || !result)
        {
            _messenger.Send(SnackBarMessage.Create($"Search returned invalid analysis result"));
            _logger.LogError("Invalid SearchWorker Result: Stage 3 returned false");
            SearchCleanup();
            return;
        }

        SearchFinished();
    }

    private void SearchCleanup()
    {
        if (!string.IsNullOrEmpty(SelectedConfig!.WorkingDirectory)
            && Directory.Exists(SelectedConfig.WorkingDirectory))
        {
            Directory.Delete(SelectedConfig.WorkingDirectory, true);
            SelectedConfig.WorkingDirectory = null;
            _logger.LogDebug("Removed Working Directory: {SelectedConfig.WorkingDirectory}", SelectedConfig.WorkingDirectory);
        }
        HideProgressIndicator();
        CancelSearchCommand.NotifyCanExecuteChanged();
        PerformSearchCommand.NotifyCanExecuteChanged();
    }

    private void SearchFinished()
    {
        HideProgressIndicator();
        SearchComplete = true;
    }

    partial void OnSearchCompleteChanged(bool oldValue, bool newValue)
    {
        if (newValue is true && newValue != oldValue)
        {
            _messenger.Send(SearchCompletedMessage.Create());
            _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
        }
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

    public async void Receive(SearchCompletedMessage message)
    {
        if (LoadingResultsCommand.IsRunning)
            return;

        await LoadingResultsCommand.ExecuteAsync(null);
    }

    #endregion

    #region Step2 - View Results

    private readonly CollectionViewSource _mediaFilesViewSource;

    [ObservableProperty]
    private ICollectionView _mediaFilesView;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    private ObservableCollection<MediaFile> _discoveredFiles = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    private string? _exportDirectory;

    [ObservableProperty]
    private ExportType _exportType;

    [ObservableProperty]
    private bool _exportRename;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackToSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelExportCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToCompletionCommand))]
    private bool _exportComplete;

    [ObservableProperty]
    private bool? _filterByShouldExport;

    [RelayCommand]
    public async Task OnLoadingResults()
    {
        if (!SearchComplete)
        {
            return;
        }

        ShowProgressIndicator("Populating Results...");
        foreach (var mediaFile in DiscoveredFiles)
        {
            mediaFile.PropertyChanged -= MediaFile_PropertyChanged;
        }
        DiscoveredFiles.Clear();
        await foreach (var file in _dbContext.FileDetails.AsAsyncEnumerable())
        {
            var mediaFile = MediaFile.Create(file);
            mediaFile.PropertyChanged += MediaFile_PropertyChanged;
            DiscoveredFiles.Add(mediaFile);
        }
        HideProgressIndicator();
    }

    private bool MediaFile_Filter_ShouldExport(object file)
        => FilterByShouldExport is null
            || (file is MediaFile mf && mf.ShouldExport == FilterByShouldExport);

    partial void OnExportDirectoryChanged(string? value)
    {
        ExportComplete = false;
    }

    partial void OnExportRenameChanged(bool value)
    {
        ExportComplete = false;
    }

    partial void OnExportTypeChanged(ExportType value)
    {
        ExportComplete = false;
    }

    partial void OnFilterByShouldExportChanged(bool? value)
    {
        MediaFilesView.Refresh();
    }

    [ObservableProperty]
    private MediaFile? _selectedExportFile;

    [RelayCommand]
    public void OnShowFileDetails(DrawerHost drawerHost)
    {
        if (SelectedExportFile is null)
            return;

        drawerHost!.IsRightDrawerOpen = true;
    }

    public bool CanNavigateBackToSearch()
        => !_exportWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanNavigateBackToSearch))]
    public void OnBackToSearch()
    {
        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Previous));
    }

    public bool CanExportFiles()
        => !string.IsNullOrEmpty(ExportDirectory)
            && DiscoveredFiles.Any(x => x.ShouldExport)
            && !_exportWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanExportFiles))]
    public async Task OnExportFiles(CancellationToken cancellationToken)
    {
        if (ExportComplete)
        {
            _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
        }

        if (ExportDirectory is null)
        {
            _messenger.Send(SnackBarMessage.Create("No export directory selected"));
            return;
        }

        if (!_exportWorker.IsBusy)
        {
            ShowProgressIndicator("Initializing export...");

            var originalLazyLoadSetting = _dbContext.ChangeTracker.LazyLoadingEnabled;
            _dbContext.ChangeTracker.LazyLoadingEnabled = false;
            var filesToExport = await _dbContext.FileDetails
                .Include(fd => fd.FileProperties)
                .Where(fd => fd.ShouldExport)
                .ToListAsync(cancellationToken);
            _dbContext.ChangeTracker.LazyLoadingEnabled = originalLazyLoadSetting;

            _exportWorker.RunWorkerAsync(ExportRequest.Create(filesToExport, ExportDirectory!, ExportType, ExportRename));
            CancelExportCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanCancelExport()
        => !ExportComplete &&
            (_exportWorker.IsBusy && !_exportWorker.CancellationPending);

    [RelayCommand(CanExecute = nameof(CanCancelExport))]
    private void OnCancelExport()
    {
        if (!_exportWorker.IsBusy)
        {
            return;
        }

        _exportWorker.CancelAsync();

        CancelExportCommand.NotifyCanExecuteChanged();
        _messenger.Send(UpdateProgressBarStatus.Create("Cancelling..."));
    }

    private void ExportCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            _messenger.Send(SnackBarMessage.Create("Export cancelled"));
            _logger.LogInformation("Process cancelled by user.");
            ExportCleanup();
            return;
        }
        if (e.Error is not null)
        {
            _messenger.Send(SnackBarMessage.Create($"Export failed: {e.Error.Message}"));
            _logger.LogError(e.Error, "Process Failed.");
            ExportCleanup();
            return;
        }
        if (e.Result is not bool result || !result)
        {
            _messenger.Send(SnackBarMessage.Create($"Export returned invalid analysis result"));
            _logger.LogError("Invalid ExportWorker Result");
            ExportCleanup();
            return;
        }

        _messenger.Send(SnackBarMessage.Create("Export completed successfully"));
        _logger.LogError("Export completed successfully");
        ExportCleanup(true);
    }

    private void ExportCleanup(bool isComplete = false)
    {
        HideProgressIndicator();
        ExportComplete = isComplete;
        ExportFilesCommand.NotifyCanExecuteChanged();
    }

    partial void OnExportCompleteChanged(bool oldValue, bool newValue)
    {
        if (newValue is true && newValue != oldValue)
        {
            _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
        }
    }

    private bool CanMoveToCompletion()
        => ExportComplete
            && !_exportWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanMoveToCompletion))]
    private void OnMoveToCompletion()
    {
        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
    }

    private async void MediaFile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not MediaFile item || e.PropertyName is null)
        {
            return;
        }

        var entity = await _dbContext.FileDetails.FindAsync(item.Id);
        if (entity is null)
        {
            return;
        }

        var itemPropertyDescriptor = TypeDescriptor.GetProperties(item)[e.PropertyName];
        var entityPropertyDescriptor = TypeDescriptor.GetProperties(entity)[e.PropertyName];
        if (entityPropertyDescriptor is not null && itemPropertyDescriptor is not null)
        {
            var originalValue = entityPropertyDescriptor.GetValue(entity);
            var newValue = itemPropertyDescriptor.GetValue(item);
            if (newValue != originalValue)
            {
                entityPropertyDescriptor.SetValue(entity, newValue);
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    #endregion

    #region Step3 - Complete

    [RelayCommand]
    public void OnBackToExport()
    {
        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Previous));
    }

    public bool CanFinishSearach()
        => (SelectedConfig?.WorkingDirectory is not null || _dbContext.FileDetails.Any())
            && !_searchStageOneWorker.IsBusy
            && !_searchStagaeTwoWorker.IsBusy
            && !_searchStagaeThreeWorker.IsBusy
            && !_exportWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanFinishSearach))]
    public async Task Finish()
    {
        _logger.LogInformation("Cleaning up search session.");
        await TruncateFileDetailState(_dbContext);

        if (SelectedConfig?.WorkingDirectory is not null
            && Directory.Exists(SelectedConfig.WorkingDirectory))
        {
            Directory.Delete(SelectedConfig.WorkingDirectory, true);
            _logger.LogDebug("Removed Working Directory: {SelectedConfig.WorkingDirectory}", SelectedConfig.WorkingDirectory);
        }

        if (SearchComplete)
        {
            SearchComplete = false;
            SelectedConfig = null;
        }

        if (ExportComplete)
        {
            ExportComplete = false;
            ExportDirectory = null;
        }

        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Beginning));
    }

    #endregion

    private static async Task TruncateFileDetailState(AppDbContext dbContext)
    {
        await dbContext.FileDetails.Include(fd => fd.FileProperties).ExecuteDeleteAsync();
        dbContext.ChangeTracker.Clear();
    }
}
#pragma warning restore CA2254 // Template should be a static expression