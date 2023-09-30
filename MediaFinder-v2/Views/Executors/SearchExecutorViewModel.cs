using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Messages;
using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2.Views.Executors;

public partial class SearchExecutorViewModel : ObservableObject, IRecipient<SearchSettingLoaded>
{
    private readonly AppDbContext _dbContext;
    private readonly IMessenger _messenger;

    public ISnackbarMessageQueue MessageQueue { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private SearchSettingItemViewModel? _searchSettings;

    public SearchExecutorViewModel(AppDbContext dbContext, IMessenger messenger, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _dbContext = dbContext;
        _messenger = messenger;
        messenger.RegisterAll(this);
        MessageQueue = snackbarMessageQueue;
    }

    public void Receive(SearchSettingLoaded message)
    {
        SearchSettings = message.Settings;
    }

    #region Step1 - Set Working Directory

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private string? _workingDirectory;

    public bool WorkingDirectoryIsSet()
        => !string.IsNullOrEmpty(WorkingDirectory) &&
            SearchSettings is not null;

    [RelayCommand(CanExecute = nameof(WorkingDirectoryIsSet), IncludeCancelCommand = true)]
    public async Task OnPerformSearch(CancellationToken cancellationToken)
    {
        try
        {
            DiscoveredFiles.Clear();
            await Task.Delay(20_000, cancellationToken);
            await foreach (var file in _dbContext.FileDetails.AsAsyncEnumerable())
            {
                DiscoveredFiles.Add(new MediaFile(file));
            }
            DiscoveredFiles.Add(new MediaFile(new DataAccessLayer.Models.FileDetails
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
            }));
            Transitioner.MoveNextCommand.Execute(null, null);
        }
        catch (TaskCanceledException)
        {
            MessageQueue.Enqueue("Search cancelled");
        }
    }

    #endregion

    #region Step2 - View Results

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    private ObservableCollection<MediaFile> _discoveredFiles = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    private string? _exportDirectory;

    [RelayCommand(CanExecute = nameof(CanExportFiles), IncludeCancelCommand = true)]
    public async Task OnExportFiles(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(20_000, cancellationToken);
            MessageQueue.Enqueue("Export completed successfully");
            Transitioner.MoveFirstCommand.Execute(null, null);
        }
        catch(TaskCanceledException)
        {
            MessageQueue.Enqueue("Export cancelled");
        }
    }

    public bool CanExportFiles()
        => !string.IsNullOrEmpty(ExportDirectory) &&
            DiscoveredFiles.Any(x => x.ShouldExport);

    #endregion
}
