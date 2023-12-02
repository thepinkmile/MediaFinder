using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using MediaFinder.DataAccessLayer;
using MediaFinder.Helpers;
using MediaFinder.Logging;
using MediaFinder.Messages;
using MediaFinder.Models;
using MediaFinder.Services.Export;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace MediaFinder.Views.Export;

public partial class ExportViewModel : ProgressableViewModel,
    IRecipient<SearchCompletedMessage>,
    IRecipient<FinishedMessage>
{
    private static readonly string SystemDrive = Path.GetPathRoot(Environment.SystemDirectory)!;
    private static IEnumerable<string> LogicalDrives => Environment
        .GetLogicalDrives()
        .Where(x => !string.Equals(x, SystemDrive, StringComparison.InvariantCultureIgnoreCase));

    private readonly ILogger<ExportViewModel> _logger;
    private readonly ExportWorker _exportWorker;

    public ExportViewModel(
        IMessenger messenger,
        ILogger<ExportViewModel> logger,
        MediaFinderDbContext dbContext,
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
        // TODO: this would break the existing filtering methods
        //MediaFilesView = CollectionViewSource.GetDefaultView(DiscoveredFiles);

        // default filters
        TypeFilter = MultiMediaType.All;
        ExportingFilter = TriStateBoolean.True;
        CreatedAfterFilter = null;
        CreatedBeforeFilter = null;

        // attach filter delegates
        _mediaFilesViewSource.Filter += ApplyExportingFilter;
        _mediaFilesViewSource.Filter += ApplyMediaTypeFilter;
        _mediaFilesViewSource.Filter += ApplyCreatedAfterFilter;
        _mediaFilesViewSource.Filter += ApplyCreatedBeforeilter;

        _exportWorker.RunWorkerCompleted += ExportCompleted;

        ExportDirectory = LogicalDrives.Any()
            ? LogicalDrives.First()
            : Path.GetTempPath();
    }

    #region Filtering

    #region Media Type Filter

    [ObservableProperty]
    private MultiMediaType _typeFilter;

    partial void OnTypeFilterChanged(MultiMediaType value)
    {
        MediaFilesView.Refresh();
    }

    private void ApplyMediaTypeFilter(object sender, FilterEventArgs e)
    {
        if (!e.Accepted || e.Item is not MediaFile item)
        {
            e.Accepted = false;
            return;
        }

        if (TypeFilter == MultiMediaType.All)
        {
            e.Accepted &= true;
            return;
        }

        if (TypeFilter == MultiMediaType.None)
        {
            e.Accepted &= false;
            return;
        }

        e.Accepted &= TypeFilter.HasFlagFast(item.MultiMediaType);
    }

    #endregion

    #region IsExporting Filter

    [ObservableProperty]
    private TriStateBoolean _exportingFilter;

    partial void OnExportingFilterChanged(TriStateBoolean value)
    {
        MediaFilesView.Refresh();
    }

    private void ApplyExportingFilter(object sender, FilterEventArgs e)
    {
        if (!e.Accepted || e.Item is not MediaFile item)
        {
            e.Accepted = false;
            return;
        }

        if (ExportingFilter == TriStateBoolean.All)
        {
            e.Accepted &= true;
            return;
        }

        if (ExportingFilter == TriStateBoolean.None)
        {
            e.Accepted &= false;
            return;
        }
        
        var itemExportFlag = item.ShouldExport ? TriStateBoolean.True : TriStateBoolean.False;
        e.Accepted &= ExportingFilter.HasFlagFast(itemExportFlag);
    }

    #endregion

    #region Created After Filter

    [ObservableProperty]
    private DateTime? _createdAfterFilter;

    partial void OnCreatedAfterFilterChanged(DateTime? value)
    {
        MediaFilesView.Refresh();
    }

    private void ApplyCreatedAfterFilter(object sender, FilterEventArgs e)
    {
        if (!e.Accepted || e.Item is not MediaFile item)
        {
            e.Accepted = false;
            return;
        }

        if (!CreatedAfterFilter.HasValue || CreatedAfterFilter == DateTime.MinValue)
        {
            e.Accepted &= true;
            return;
        }

        e.Accepted &= CreatedAfterFilter.Value.Date <= item.DateCreated.DateTime.Date;
    }

    #endregion

    #region Created Before Filter

    [ObservableProperty]
    private DateTime? _createdBeforeFilter;

    partial void OnCreatedBeforeFilterChanged(DateTime? value)
    {
        MediaFilesView.Refresh();
    }

    private void ApplyCreatedBeforeilter(object sender, FilterEventArgs e)
    {
        if (!e.Accepted || e.Item is not MediaFile item)
        {
            e.Accepted = false;
            return;
        }

        if (!CreatedBeforeFilter.HasValue || CreatedBeforeFilter == DateTime.MaxValue)
        {
            e.Accepted &= true;
            return;
        }

        e.Accepted &= CreatedBeforeFilter.Value.Date >= item.DateCreated.DateTime.Date;
    }

    #endregion

    #endregion

    #region Step2 - View Results

    public async void Receive(SearchCompletedMessage message)
    {
        try
        {
            if (LoadingResultsCommand.IsRunning)
                return;

            await LoadingResultsCommand.ExecuteAsync(null).ConfigureAwait(true);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to handle Search Completed message.");
        }
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
    private ObservableCollection<MediaFile> _discoveredFiles = [];

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

    [RelayCommand]
    public async Task OnLoadingResults()
    {
        ShowProgressIndicator("Populating Results...");
        DiscoveredFiles.Clear();
        await foreach (var file in _dbContext.FileDetails.AsAsyncEnumerable())
        {
            var mediaFile = file.ToMediaFile();
            DiscoveredFiles.Add(mediaFile);
        }
        MediaFilesTotalCount = DiscoveredFiles.Count;
        MediaFilesViewCount = DiscoveredFiles.Count(x => x.ShouldExport);
        HideProgressIndicator();
    }

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

    [ObservableProperty]
    private MediaFile? _selectedExportFile;

    [RelayCommand]
    public async Task OnToggleExportFlag(MediaFile item)
    {
        var entity = await _dbContext.FileDetails
            .FindAsync(item.Id)
            .ConfigureAwait(true);
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
                await _dbContext.SaveChangesAsync().ConfigureAwait(true);
                MediaFilesViewCount = DiscoveredFiles.Count(x => x.ShouldExport);
                ExportFilesCommand.NotifyCanExecuteChanged();
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
                .ToListAsync(cancellationToken)
                .ConfigureAwait(true);
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
            _logger.UserCanceledOperation("Export");
            ExportCleanup();
            return;
        }
        if (e.Error is not null)
        {
            _messenger.Send(SnackBarMessage.Create("Export failed"));
            _logger.ProcessFailed(e.Error);
            ExportCleanup();
            return;
        }
        if (e.Result is not bool result || !result)
        {
            _messenger.Send(SnackBarMessage.Create($"Export returned invalid analysis result"));
            _logger.InvalidResult("ExportWorker", e.Result!.GetType());
            ExportCleanup();
            return;
        }

        _messenger.Send(SnackBarMessage.Create("Export completed successfully"));
        _logger.ExportComplete();
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

    public bool CanFinishSearch()
        => !_exportWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanFinishSearch))]
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
            _logger.SessionCleanup();
            ExportComplete = false;
            ExportDirectory = null;
        }
    }

    #endregion
}
