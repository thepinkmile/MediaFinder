using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Helpers;
using MediaFinder_v2.Messages;

using MediaFinder_v2.Services.Export;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Collections.ObjectModel;
using System.ComponentModel;

using System.IO;
using System.Windows.Data;

namespace MediaFinder_v2.Views.Export;

[ObservableObject]
public partial class ExportViewModel : ProgressableViewModel,
    IRecipient<SearchCompletedMessage>,
    IRecipient<FinishedMessage>
{
    private readonly ILogger<ExportViewModel> _logger;
    private readonly ExportWorker _exportWorker;

    public ExportViewModel(
        IMessenger messenger,
        ILogger<ExportViewModel> logger,
        AppDbContext dbContext,
        ExportWorker exportWorker)
        : base(messenger, logger, dbContext)
    {
        _logger = logger;
        _exportWorker = exportWorker;

        messenger.RegisterAll(this);

        BindingOperations.EnableCollectionSynchronization(DiscoveredFiles, new());

        // Bind CollectionViewSource.Source to MyCollection
        _mediaFilesViewSource = new CollectionViewSource();
        Binding myBind = new() { Source = DiscoveredFiles };
        BindingOperations.SetBinding(_mediaFilesViewSource, CollectionViewSource.SourceProperty, myBind);
        MediaFilesView = _mediaFilesViewSource.View;
        MediaFilesView.Filter = MediaFile_Filter_ShouldExport;
        FilterByShouldExport = true;

        _exportWorker.RunWorkerCompleted += ExportCompleted;

        ExportDirectory = Environment.GetLogicalDrives().Length > 1
            ? Environment.GetLogicalDrives().Skip(1).First()
            : Path.GetTempPath();
    }

    #region Step2 - View Results

    public async void Receive(SearchCompletedMessage message)
    {
        if (LoadingResultsCommand.IsRunning)
            return;

        await LoadingResultsCommand.ExecuteAsync(null);
    }

    private readonly CollectionViewSource _mediaFilesViewSource;

    [ObservableProperty]
    private ICollectionView _mediaFilesView;

    [ObservableProperty]
    private int _mediaFilesViewCount;

    [ObservableProperty]
    private int _mediaFilesTotalCount;

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
    [NotifyCanExecuteChangedFor(nameof(NavigateBackCommand))]
    [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelExportCommand))]
    [NotifyCanExecuteChangedFor(nameof(NavigateNextCommand))]
    private bool _exportComplete;

    [ObservableProperty]
    private bool? _filterByShouldExport;

    [RelayCommand]
    public async Task OnLoadingResults()
    {
        ShowProgressIndicator("Populating Results...");
        DiscoveredFiles.Clear();
        await foreach (var file in _dbContext.FileDetails.AsAsyncEnumerable())
        {
            var mediaFile = MediaFile.Create(file);
            DiscoveredFiles.Add(mediaFile);
        }
        MediaFilesTotalCount = DiscoveredFiles.Count;
        MediaFilesViewCount = DiscoveredFiles.Count(x => x.ShouldExport);
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
    public async Task OnToggleExportFlag(MediaFile item)
    {
        var entity = await _dbContext.FileDetails.FindAsync(item.Id);
        if (entity is null)
        {
            return;
        }

        var entityPropertyDescriptor = TypeDescriptor.GetProperties(entity)[nameof(MediaFile.ShouldExport)];
        if (entityPropertyDescriptor is not null)
        {
            var originalValue = (bool)entityPropertyDescriptor.GetValue(entity)!;
            var newValue = !item.ShouldExport;
            if (newValue != originalValue)
            {
                item.ShouldExport = newValue;
                entityPropertyDescriptor.SetValue(entity, newValue);
                await _dbContext.SaveChangesAsync();

                MediaFilesViewCount = DiscoveredFiles.Count(x => x.ShouldExport);
                OnPropertyChanged(nameof(DiscoveredFiles));
                MediaFilesView.Refresh();
            }
        }
    }

    [RelayCommand]
    public void OnShowFileDetails(DrawerHost drawerHost)
    {
        if (SelectedExportFile is null)
            return;

        drawerHost!.IsRightDrawerOpen = true;
    }

    public bool CanNavigateBack()
        => !_exportWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    public void OnNavigateBack()
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
            ShowProgressIndicator("Initializing export...", CancelExportCommand);

            var originalLazyLoadSetting = _dbContext.ChangeTracker.LazyLoadingEnabled;
            _dbContext.ChangeTracker.LazyLoadingEnabled = false;
            var filesToExport = await _dbContext.FileDetails
                .Include(fd => fd.FileProperties)
                .Where(fd => fd.ShouldExport)
                .ToListAsync(cancellationToken);
            _dbContext.ChangeTracker.LazyLoadingEnabled = originalLazyLoadSetting;

            _exportWorker.RunWorkerAsync(
                ExportRequest.Create(
                    _progressToken,
                    filesToExport,
                    ExportDirectory!,
                    ExportType, ExportRename));
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

        CancelProgressIndicator("Cancelling...");
        _exportWorker.CancelAsync();
        CancelExportCommand.NotifyCanExecuteChanged();
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
        CancelExportCommand.NotifyCanExecuteChanged();
    }

    partial void OnExportCompleteChanged(bool oldValue, bool newValue)
    {
        if (newValue is true && newValue != oldValue)
        {
            _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
        }
    }

    private bool CanNavigateNext()
        => ExportComplete
            && !_exportWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private void OnNavigateNext()
    {
        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
    }

    #endregion

    #region Finish

    public bool CanFinishSearach()
        => !_exportWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanFinishSearach))]
    public void Finish()
    {
        _messenger.Send(FinishedMessage.Create());
    }

    public void Receive(FinishedMessage message)
    {
        Cleanup();
    }

    internal void Cleanup()
    {
        if (ExportComplete)
        {
            _logger.LogInformation("Cleaning up export session.");
            ExportComplete = false;
            ExportDirectory = null;
        }
    }

    #endregion
}
