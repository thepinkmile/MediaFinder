using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf.Transitions;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Helpers;
using MediaFinder_v2.Messages;
using MediaFinder_v2.Services.Search;
using MediaFinder_v2.Views.SearchSettings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Views.Executors;

#pragma warning disable CA2254 // Template should be a static expression - Don't care about formats here
public partial class SearchExecutorViewModel : ObservableObject, IRecipient<WorkingDirectoryCreated>
{
    private readonly IServiceProvider _serviceProvider;

    private readonly AppDbContext _dbContext;
    private readonly IMessenger _messenger;
    private readonly ILogger<SearchExecutorViewModel> _logger;

    private readonly SearchStageOneWorker _searchStageOneWorker;
    private readonly SearchStageTwoWorker _searchStagaeTwoWorker;

    public SearchExecutorViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _messenger = _serviceProvider.GetRequiredService<IMessenger>();
        _logger = _serviceProvider.GetRequiredService<ILogger<SearchExecutorViewModel>>();
        _searchStageOneWorker = _serviceProvider.GetRequiredService<SearchStageOneWorker>();
        _searchStagaeTwoWorker = _serviceProvider.GetRequiredService<SearchStageTwoWorker>();

        _messenger.RegisterAll(this);

        BindingOperations.EnableCollectionSynchronization(Configurations, new());
        BindingOperations.EnableCollectionSynchronization(DiscoveredFiles, new());
        DiscoveredFiles.ListChanged += DiscoveredFiles_ListChanged;
        _searchStageOneWorker.RunWorkerCompleted += SearchStepOneCompleted;
        _searchStagaeTwoWorker.RunWorkerCompleted += SearchStageTwoCompleted;
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

    #region Step1 - Set Working Directory

    [ObservableProperty]
    private ObservableCollection<SearchSettingItemViewModel> _configurations = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private string? _workingDirectory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private SearchSettingItemViewModel? _selectedConfig;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(BackToSearchCommand))]
    private bool _searchComplete;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
    private bool _isSearching;

    partial void OnWorkingDirectoryChanged(string? value)
    {
        SearchComplete = false;
    }

    partial void OnSelectedConfigChanged(SearchSettingItemViewModel? value)
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
            Configurations.Add(new SearchSettingItemViewModel(config));
        }

        HideProgressIndicator();

        if (SearchComplete && !_searchStageOneWorker.IsBusy)
        {
            await MoveToReviewCommand.ExecuteAsync(null);
        }
    }

    public bool CanPerformSearch()
        => !string.IsNullOrEmpty(WorkingDirectory)
            && SelectedConfig is not null
            && !SearchComplete
            && !IsSearching
            && !_searchStageOneWorker.IsBusy;

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

        IsSearching = true;
        ShowProgressIndicator("Initializing search parameters");

        if (!_searchStageOneWorker.IsBusy)
        {
            _searchStageOneWorker.RunWorkerAsync(SearchRequest.Create(WorkingDirectory!, SelectedConfig!));
            CancelSearchCommand.NotifyCanExecuteChanged();
        }

        //try
        //{
            
        //}
        //catch (TaskCanceledException)
        //{
        //    _messenger.Send(SnackBarMessage.Create("Search cancelled"));
        //    _logger.LogInformation("Process cancelled by user.");
        //}
        //catch (Exception ex)
        //{
        //    _messenger.Send(SnackBarMessage.Create($"Search failed: {ex.Message}"));
        //    _logger.LogError(ex, "Process Failed.");
        //}
        //finally
        //{
        //    HideProgressIndicator();
        //    IsSearching = false;
        //}

        #region Original Code

        //ShowProgressIndicator("Preparing Working Directory...");
        //var workingDirectory = Path.Combine(WorkingDirectory!, Guid.NewGuid().ToString());
        //Directory.CreateDirectory(workingDirectory);
        //SelectedConfig.WorkingDirectory = workingDirectory;
        //WorkingDirectorySet = true;

        //try
        //{
        //    await TruncateFileDetailState(_dbContext, cancellationToken);

        //    UpdateProgressIndicator("Performing Search...");
        //    await foreach (var file in _mediaLocator.Search(SelectedConfig.Directories, recursive: SelectedConfig.Recursive, performDeepAnalysis: SelectedConfig.PerformDeepAnalysis, cancellationToken: cancellationToken))
        //    {
        //        _dbContext.FileDetails.Add(file);
        //    }
        //    await _dbContext.SaveChangesAsync(cancellationToken);

        //    if (SelectedConfig.ExtractArchives)
        //    {
        //        UpdateProgressIndicator("Extracting Archives...");

        //        var extractionDepth = 0;
        //        bool filesExtracted;
        //        do
        //        {
        //            extractionDepth++;
        //            filesExtracted = false;

        //            var archiveCount = await _dbContext.FileDetails.CountAsync(f => f.FileType == DataAccessLayer.Models.MultiMediaType.Archive && !f.Extracted, cancellationToken);
        //            var currentArchive = 0;
        //            foreach (var archive in _dbContext.FileDetails.Where(f => f.FileType == DataAccessLayer.Models.MultiMediaType.Archive && !f.Extracted))
        //            {
        //                cancellationToken.ThrowIfCancellationRequested();

        //                var filePath = Path.Combine(archive.ParentPath, archive.FileName);
        //                UpdateProgressIndicator($"Extracting Archives ...\nIteration: {extractionDepth}\nExtracting {++currentArchive} of {archiveCount}\nFile: {filePath}");
        //                var destDir = Path.Combine(workingDirectory, $"Extracted_{archive.FileName}");

        //                _logger.LogDebug("Extracting archive: {filePath}", filePath);
        //                _archiveExtractor.Extract(filePath, destDir);
        //                archive.Extracted = true;
        //                _logger.LogDebug("Archive extracted: {filePath}", filePath);

        //                await foreach (var file in _mediaLocator.Search(destDir, recursive: SelectedConfig.Recursive, performDeepAnalysis: SelectedConfig.PerformDeepAnalysis, cancellationToken: cancellationToken))
        //                {
        //                    filesExtracted = true;
        //                    _dbContext.FileDetails.Add(file);
        //                }
        //                _logger.LogDebug("Discovered files in extraction directory: {destDir}", destDir);
        //            }
        //            await _dbContext.SaveChangesAsync(cancellationToken);
        //        }
        //        while (filesExtracted && extractionDepth <= SelectedConfig.ExtractionDepth);
        //    }

        //    if (SelectedConfig.PerformDeepAnalysis)
        //    {
        //        UpdateProgressIndicator("Analysing Files...");
        //        var totalFiles = await _dbContext.FileDetails
        //            .CountAsync(f => f.FileType != DataAccessLayer.Models.MultiMediaType.Archive, cancellationToken);
        //        var currentFile = 0;
        //        await foreach (var file in _dbContext.FileDetails
        //            .Include(fd => fd.FileProperties)
        //            .Where(f => f.FileType != DataAccessLayer.Models.MultiMediaType.Archive)
        //            .AsAsyncEnumerable())
        //        {
        //            cancellationToken.ThrowIfCancellationRequested();

        //            var filePath = Path.Combine(file.ParentPath, file.FileName);
        //            UpdateProgressIndicator($"Analysing File {++currentFile} of {totalFiles}\nFile: {filePath}");

        //            var detector = _mediaDetectors.SingleOrDefault(md => md.MediaType == file.FileType);
        //            if (detector is not null)
        //            {
        //                var details = detector.GetMediaProperties(filePath);
        //                file.FileProperties = details
        //                    .Where(kvp => kvp.Value != null && kvp.Key != null)
        //                    .Select(kvp => new DataAccessLayer.Models.FileProperty
        //                    {
        //                        Name = kvp.Key,
        //                        Value = kvp.Value
        //                    }).ToList();
        //            }
        //        }
        //        await _dbContext.SaveChangesAsync(cancellationToken);
        //    }

        //    UpdateProgressIndicator("Calculating Hashes...");
        //    var total = await _dbContext.FileDetails
        //        .CountAsync(f => f.FileType != DataAccessLayer.Models.MultiMediaType.Archive, cancellationToken);
        //    var current = 0;
        //    await foreach (var file in _dbContext.FileDetails
        //        .Where(f => f.FileType != DataAccessLayer.Models.MultiMediaType.Archive)
        //        .AsAsyncEnumerable())
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        var filePath = Path.Combine(file.ParentPath, file.FileName);
        //        UpdateProgressIndicator($"Calculating Hash {++current} of {total}\nFile: {filePath}");
        //        var fileInfo = new FileInfo(filePath);

        //        using var fileStream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        //        using var hashStream = new HashStream(fileStream, HashAlgorithmName.MD5, HashAlgorithmName.SHA256, HashAlgorithmName.SHA512);

        //        var read = 1024;
        //        var buffer = new byte[read];
        //        do
        //        {
        //            await Task.Yield();
        //            cancellationToken.ThrowIfCancellationRequested();
        //            read = hashStream.Read(buffer, 0, buffer.Length);
        //        } while (read != 0);

        //        file.MD5_Hash = Convert.ToHexString(hashStream.Hash(HashAlgorithmName.MD5));
        //        file.SHA256_Hash = Convert.ToHexString(hashStream.Hash(HashAlgorithmName.SHA256));
        //        file.SHA512_Hash = Convert.ToHexString(hashStream.Hash(HashAlgorithmName.SHA512));

        //        _logger.LogDebug("File hash for '{filePath}': MD5 = {MD5_Hash}", filePath, file.MD5_Hash);
        //        _logger.LogDebug("File hash for '{filePath}': SHA256 = {SHA256_Hash}", filePath, file.SHA256_Hash);
        //        _logger.LogDebug("File hash for '{filePath}': SHA512 = {SHA512_Hash}", filePath, file.SHA512_Hash);
        //    }
        //    await _dbContext.SaveChangesAsync(cancellationToken);

        //    UpdateProgressIndicator("Saving Results...");
        //    await foreach (var file in _dbContext.FileDetails
        //        .Where(f => f.FileType == DataAccessLayer.Models.MultiMediaType.Image || f.FileType == DataAccessLayer.Models.MultiMediaType.Video)
        //        .AsAsyncEnumerable())
        //    {
        //        file.ShouldExport = true;
        //    }
        //    await _dbContext.SaveChangesAsync(cancellationToken);

        //    var tmp = await _dbContext.FileDetails.CountAsync(fd => fd.ShouldExport == true, cancellationToken);

        //    UpdateProgressIndicator("Removing Duplicates...");
        //    var md5DupeCount = 0;
        //    var md5_hashes = await _dbContext.FileDetails
        //        .Where(f => f.ShouldExport && f.MD5_Hash != null)
        //        .GroupBy(f => f.MD5_Hash)
        //        .Where(g => g.Count() > 1)
        //        .Select(g => g.Key)
        //        .ToListAsync(cancellationToken);
        //    UpdateProgressIndicator($"Removing Duplicates...\nBased on MD5 Hash: {md5_hashes.Count}");
        //    foreach (var md5_hash in md5_hashes)
        //    {
        //        await foreach (var file in _dbContext.FileDetails
        //            .Where(f => f.MD5_Hash == md5_hash)
        //            .Skip(1)
        //            .AsAsyncEnumerable())
        //        {
        //            file.ShouldExport = false;
        //            ++md5DupeCount;
        //        }
        //    }
        //    await _dbContext.SaveChangesAsync(cancellationToken);
        //    var sha256DupeCount = 0;
        //    var sha256_hashes = await _dbContext.FileDetails
        //        .Where(f => f.ShouldExport && f.SHA256_Hash != null)
        //        .GroupBy(f => f.SHA256_Hash)
        //        .Where(g => g.Count() > 1)
        //        .Select(g => g.Key)
        //        .ToListAsync(cancellationToken);
        //    UpdateProgressIndicator($"Removing Duplicates...\nBased on SHA256 Hash: {sha256_hashes.Count}");
        //    foreach (var sha256_hash in sha256_hashes)
        //    {
        //        await foreach (var file in _dbContext.FileDetails
        //            .Where(f => f.SHA256_Hash == sha256_hash)
        //            .Skip(1)
        //            .AsAsyncEnumerable())
        //        {
        //            file.ShouldExport = false;
        //            ++sha256DupeCount;
        //        }
        //    }
        //    await _dbContext.SaveChangesAsync(cancellationToken);
        //    var sha512DupeCount = 0;
        //    var sha512_hashes = await _dbContext.FileDetails
        //        .Where(f => f.ShouldExport && f.SHA512_Hash != null)
        //        .GroupBy(f => f.SHA512_Hash)
        //        .Where(g => g.Count() > 1)
        //        .Select(g => g.Key)
        //        .ToListAsync(cancellationToken);
        //    UpdateProgressIndicator($"Removing Duplicates...\nBased on SHA512 Hash: {sha512_hashes.Count}");
        //    foreach (var sha512_hash in sha512_hashes)
        //    {
        //        await foreach (var file in _dbContext.FileDetails
        //            .Where(f => f.SHA512_Hash == sha512_hash)
        //            .Skip(1)
        //            .AsAsyncEnumerable())
        //        {
        //            file.ShouldExport = false;
        //            ++sha512DupeCount;
        //        }
        //    }
        //    await _dbContext.SaveChangesAsync(cancellationToken);

        //    var tmp2 = await _dbContext.FileDetails.CountAsync(fd => fd.ShouldExport == true, cancellationToken);
        //    var totalDupeCount = md5DupeCount + sha256DupeCount + sha512DupeCount;
        //    _logger.LogDebug("Original Export Count: {tmp}", tmp);
        //    _logger.LogDebug("MD5 Duplicates Removed: {md5DupeCount}", md5DupeCount);
        //    _logger.LogDebug("SHA256 Duplicates Removed: {sha256DupeCount}", sha256DupeCount);
        //    _logger.LogDebug("SHA512 Duplicates Removed: {sha512DupeCount}", sha512DupeCount);
        //    _logger.LogDebug("Total Duplicates Removed: {totalDupeCount}", totalDupeCount);
        //    _logger.LogDebug("Final Export Count: {tmp2}", tmp2);
        //    UpdateProgressIndicator($"Finalizing...\nTotal Duplicates Found: {totalDupeCount}\nMD5: {md5DupeCount}\nSHA256: {sha256DupeCount}\nSHA512: {sha512DupeCount}\n\nOriginal Count: {tmp}\nFinal Count: {tmp2}");
        //    SearchComplete = true;
        //}
        //catch (TaskCanceledException)
        //{
        //    _messenger.Send(SnackBarMessage.Create("Search cancelled"));
        //    _logger.LogInformation("Process cancelled by user.");
        //}
        //catch (Exception ex)
        //{
        //    _messenger.Send(SnackBarMessage.Create($"Search failed: {ex.Message}"));
        //    _logger.LogError(ex, "Process Failed.");
        //}
        //finally
        //{
        //    HideProgressIndicator();

        //    IsSearching = false;
        //}

        #endregion
    }

    private bool CanCancelSearch()
        => IsSearching && (
            (_searchStageOneWorker.IsBusy && !_searchStageOneWorker.CancellationPending)
            || (_searchStagaeTwoWorker.IsBusy && !_searchStagaeTwoWorker.CancellationPending)
        );

    [RelayCommand(CanExecute = nameof(CanCancelSearch))]
    private void OnCancelSearch()
    {
        if (!_searchStageOneWorker.IsBusy
            && !_searchStagaeTwoWorker.IsBusy)
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

        _searchStagaeTwoWorker.RunWorkerAsync(AnalyseRequest.Creatae(result.Files, SelectedConfig!.PerformDeepAnalysis));
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

        _dbContext.FileDetails.AddRange(result.Files);
        _dbContext.SaveChanges();

        IsSearching = false;
        SearchComplete = true;
        HideProgressIndicator();
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
        IsSearching = false;
        HideProgressIndicator();
    }

    private bool CanMoveToReview()
        => SearchComplete
            && !IsSearching
            && !_searchStageOneWorker.IsBusy;

    [RelayCommand(CanExecute = nameof(CanMoveToReview))]
    private async Task OnMoveToReview()
    {
        var reload = LoadingResultsCommand.ExecuteAsync(null);
        Transitioner.MoveNextCommand.Execute(null, null);
        await reload;
    }

    #endregion

    #region Step2 - View Results

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    private AdvancedBindingList<MediaFile> _discoveredFiles = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportFilesCommand))]
    private string? _exportDirectory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackToSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
    private bool _isExporting;

    [RelayCommand]
    public async Task OnLoadingResults()
    {
        if (!SearchComplete)
        {
            return;
        }

        ShowProgressIndicator("Populating Results...");
        DiscoveredFiles.Clear();
        await foreach (var file in _dbContext.FileDetails.AsAsyncEnumerable())
        {
            DiscoveredFiles.Add(MediaFile.Create(file));
        }
        HideProgressIndicator();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    public static void OnBackToSearch(CancellationToken cancellationToken)
    {
        Transitioner.MoveFirstCommand.Execute(null, null);
    }

    public bool CanNavigateBack()
        => !IsExporting
            && SearchComplete;

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
            _logger.LogInformation("Process cancelled by user.");
        }
        catch(Exception ex)
        {   
            _messenger.Send(SnackBarMessage.Create($"Export failed: {ex.Message}"));
            _logger.LogError(ex, "Process failed.");
        }
        finally
        {
            _messenger.Send(HideProgressBar.Create());
            IsExporting = false;
        }
    }

    public bool CanExportFiles()
        => !string.IsNullOrEmpty(ExportDirectory) &&
            DiscoveredFiles.Any(x => x.ShouldExport);

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

    #endregion

    #region Step3 - Complete

    [RelayCommand]
    public static void OnBackToExport()
    {
        Transitioner.MovePreviousCommand.Execute(null, null);
    }

    public bool CanFinishSearach()
        => !IsSearching
            && !IsExporting
            && !_searchStageOneWorker.IsBusy
            && SelectedConfig?.WorkingDirectory is not null;

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
        Transitioner.MoveFirstCommand.Execute(null, null);
    }

    #endregion

    private static async Task TruncateFileDetailState(AppDbContext dbContext)
    {
        await dbContext.FileDetails.Include(fd => fd.FileProperties).ExecuteDeleteAsync();
        dbContext.ChangeTracker.Clear();
    }
}
#pragma warning restore CA2254 // Template should be a static expression