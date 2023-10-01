using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf.Transitions;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Messages;
using MediaFinder_v2.Views.SearchSettings;

using Microsoft.EntityFrameworkCore;

namespace MediaFinder_v2.Views.Executors;

public partial class SearchExecutorViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext;
    private readonly IMessenger _messenger;

    public SearchExecutorViewModel(AppDbContext dbContext, IMessenger messenger)
    {
        _dbContext = dbContext;
        _messenger = messenger;
        BindingOperations.EnableCollectionSynchronization(Configurations, new());
        BindingOperations.EnableCollectionSynchronization(DiscoveredFiles, new());
        DiscoveredFiles.ListChanged += DiscoveredFiles_ListChanged;
    }

    private async void DiscoveredFiles_ListChanged(object? sender, ListChangedEventArgs e)
    {
        if (e.ListChangedType is ListChangedType.ItemChanged)
        {
            var item = DiscoveredFiles.ElementAt(e.OldIndex);
            var entity = await _dbContext.FileDetails.FindAsync(item.Id);
            if (entity is not null && e.PropertyDescriptor is not null)
            {
                var entityPropertyDescriptor = TypeDescriptor.GetProperties(entity)[e.PropertyDescriptor.Name];
                if (entityPropertyDescriptor is not null)
                {
                    entityPropertyDescriptor!.SetValue(entity, e.PropertyDescriptor.GetValue(item));
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        ExportFilesCommand.NotifyCanExecuteChanged();
    }

    #region Step1 - Set Working Directory

    [ObservableProperty]
    private ObservableCollection<SearchSettingItemViewModel> _configurations = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private string? _workingDirectory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private SearchSettingItemViewModel? _selectedConfig;

    public bool WorkingDirectoryIsSet()
        => !string.IsNullOrEmpty(WorkingDirectory) &&
            SelectedConfig is not null;


    [RelayCommand]
    public async Task LoadConfigurations()
    {
        _messenger.Send(ShowProgressBar.Create("Loading..."));
        Configurations.Clear();

        await foreach (var config in _dbContext.SearchSettings.AsAsyncEnumerable())
        {
            Configurations.Add(new SearchSettingItemViewModel(config));
        }
        _messenger.Send(HideProgressBar.Create());
    }

    [RelayCommand(CanExecute = nameof(WorkingDirectoryIsSet), IncludeCancelCommand = true)]
    public async Task OnPerformSearch(CancellationToken cancellationToken)
    {
        // TODO: implement me properly

        _messenger.Send(ShowProgressBar.Create("Performing Search..."));
        await _dbContext.FileDetails.ExecuteDeleteAsync(cancellationToken);
        await Task.Delay(500, cancellationToken);
        // TEST DATA
        _dbContext.FileDetails.Add(new DataAccessLayer.Models.FileDetails
        {
            Id = 0,
            FileName = "Test.zip",
            ParentPath = "C:\\",
            FileSize = 10,
            FileType = DataAccessLayer.Models.MultiMediaType.Archive,
            MD5_Hash = "MD5",
            SHA256_Hash = "SHA256",
            SHA512_Hash = "SHA512",
            ShouldExport = true
        });
        // ~TEST DATA

        try
        {
            _messenger.Send(UpdateProgressBarStatus.Create("Discovering Files..."));
            DiscoveredFiles.Clear();
            await Task.Delay(5_000, cancellationToken);

            if (SelectedConfig is not null && SelectedConfig.ExtractArchives)
            {
                _messenger.Send(UpdateProgressBarStatus.Create("Extracting Archives..."));
                await Task.Delay(5_000, cancellationToken);
            }

            if (SelectedConfig is not null && SelectedConfig.PerformDeepAnalysis)
            {
                _messenger.Send(UpdateProgressBarStatus.Create("Analysing Files..."));
                await Task.Delay(5_000, cancellationToken);
            }

            // save all database items
            await _dbContext.SaveChangesAsync(cancellationToken);

            _messenger.Send(UpdateProgressBarStatus.Create("Populating Results..."));
            await foreach (var file in _dbContext.FileDetails.AsAsyncEnumerable())
            {
                DiscoveredFiles.Add(new MediaFile(file));
            }
            Transitioner.MoveNextCommand.Execute(null, null);
        }
        catch (TaskCanceledException)
        {
            _messenger.Send(SnackBarMessage.Create("Search cancelled"));
        }
        catch(Exception ex)
        {
            _messenger.Send(SnackBarMessage.Create($"Search failed: {ex.Message}"));
        }
        _messenger.Send(HideProgressBar.Create());
    }

    #endregion

    #region Step2 - View Results

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    private BindingList<MediaFile> _discoveredFiles = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    private string? _exportDirectory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    private bool _isExporting;

    [RelayCommand(CanExecute = nameof(CanNavigateBack), IncludeCancelCommand = true)]
    public async Task OnBack(CancellationToken cancellationToken)
    {
        await _dbContext.FileDetails.ExecuteDeleteAsync(cancellationToken);
        Transitioner.MoveFirstCommand.Execute(null, null);
    }

    public bool CanNavigateBack()
        => !IsExporting;

    [RelayCommand(CanExecute = nameof(CanExportFiles), IncludeCancelCommand = true)]
    public async Task OnExportFiles(CancellationToken cancellationToken)
    {
        IsExporting = true;
        _messenger.Send(ShowProgressBar.Create("Exporting Files..."));
        try
        {
            // TODO: Implement export feature
            await Task.Delay(20_000, cancellationToken);

            _messenger.Send(SnackBarMessage.Create("Export completed successfully"));
            Transitioner.MoveNextCommand.Execute(null, null);
        }
        catch(TaskCanceledException)
        {
            _messenger.Send(SnackBarMessage.Create("Export cancelled"));
        }
        catch(Exception ex)
        {   
            _messenger.Send(SnackBarMessage.Create($"Export failed: {ex.Message}"));
        }
        _messenger.Send(HideProgressBar.Create());
        IsExporting = false;
    }

    public bool CanExportFiles()
        => !string.IsNullOrEmpty(ExportDirectory) &&
            DiscoveredFiles.Any(x => x.ShouldExport);

    #endregion

    #region Step3 - Complete

    [RelayCommand]
    public static void OnBackToExport()
    {
        Transitioner.MovePreviousCommand.Execute(null, null);
    }

    [RelayCommand(IncludeCancelCommand = true)]
    public async Task Finish(CancellationToken cancellationToken)
    {
        // TODO: Perform Cleanup
        await _dbContext.FileDetails.ExecuteDeleteAsync(cancellationToken);
        Transitioner.MoveFirstCommand.Execute(null, null);
    }

    #endregion
}
