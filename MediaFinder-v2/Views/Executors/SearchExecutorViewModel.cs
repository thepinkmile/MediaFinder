using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf.Transitions;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Helpers;
using MediaFinder_v2.Messages;
using MediaFinder_v2.Services;
using MediaFinder_v2.Views.SearchSettings;

using Microsoft.EntityFrameworkCore;

using SevenZipExtractor;

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

    private async Task ShowProgressIndicator(string message, CancellationToken cancellationToken = default)
    {
        _messenger.Send(ShowProgressBar.Create(message));
        await Task.Delay(500, cancellationToken);
    }

    private async Task UpdateProgressIndicator(string message, CancellationToken cancellationToken = default)
    {
        _messenger.Send(UpdateProgressBarStatus.Create(message));
        await Task.Delay(500, cancellationToken);
    }

    private async Task HideProgressIndicator()
    {
        _messenger.Send(HideProgressBar.Create());
        await Task.Yield();
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
    [NotifyCanExecuteChangedFor(nameof(MoveToReviewCommand))]
    private bool _searchComplete;

    partial void OnWorkingDirectoryChanged(string? value)
    {
        SearchComplete = false;
    }

    partial void OnSelectedConfigChanged(SearchSettingItemViewModel? value)
    {
        SearchComplete = false;
    }

    public bool WorkingDirectoryIsSet()
        => !string.IsNullOrEmpty(WorkingDirectory) &&
            SelectedConfig is not null &&
            !SearchComplete;

    [RelayCommand]
    public async Task LoadConfigurations()
    {
        await ShowProgressIndicator("Loading...");

        Configurations.Clear();
        await foreach (var config in _dbContext.SearchSettings.AsAsyncEnumerable())
        {
            Configurations.Add(new SearchSettingItemViewModel(config));
        }

        await HideProgressIndicator();

        if (SearchComplete)
        {
            MoveToReviewCommand.Execute(null);
        }
    }

    [RelayCommand(CanExecute = nameof(WorkingDirectoryIsSet), IncludeCancelCommand = true)]
    public async Task OnPerformSearch(CancellationToken cancellationToken)
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


        await ShowProgressIndicator("Preparing Working Directory...", cancellationToken);
        var workingDirectory = Path.Combine(WorkingDirectory!, Guid.NewGuid().ToString());
        Directory.CreateDirectory(workingDirectory);
        SelectedConfig.WorkingDirectory = workingDirectory;

        try
        {
            await _dbContext.FileDetails.ExecuteDeleteAsync(cancellationToken);

            await UpdateProgressIndicator("Performing Search...", cancellationToken);
            await foreach (var file in MediaLocator.Search(SelectedConfig.Directories, recursive: SelectedConfig.Recursive, cancellationToken: cancellationToken))
            {
                _dbContext.FileDetails.Add(file);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (SelectedConfig.ExtractArchives && false)
            {
                await UpdateProgressIndicator("Extracting Archives...", cancellationToken);

                var extractionDepth = 0;
                bool filesExtracted;
                do
                {
                    extractionDepth++;
                    filesExtracted = false;

                    var archiveCount = await _dbContext.FileDetails.CountAsync(f => f.FileType == DataAccessLayer.Models.MultiMediaType.Archive && !f.Extracted, cancellationToken);
                    var currentArchive = 0;
                    await foreach (var archive in _dbContext.FileDetails.Where(f => f.FileType == DataAccessLayer.Models.MultiMediaType.Archive && !f.Extracted).AsAsyncEnumerable())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var filePath = Path.Combine(archive.ParentPath, archive.FileName);
                        await UpdateProgressIndicator($"Extracting Archives ...\nIteration: {extractionDepth}\nExtracting {++currentArchive} of {archiveCount}\nFile: {filePath}", cancellationToken);
                        var destDir = Path.Combine(workingDirectory, $"Extracted_{archive.FileName}");

                        archive.Extracted = await Task.Factory.StartNew((state) =>
                        {
                            if (state is not ExtractionState extractionState)
                            {
                                return false;
                            }

                            var result = false;
                            try
                            {
                                using var extractor = new ArchiveFile(extractionState.Source);
                                extractor.Extract(extractionState.Destination);
                                result = true;
                            }
                            catch
                            {
                                // do nothing
                            }
                            return result;
                        }, ExtractionState.Create(filePath, destDir));

                        await foreach (var file in MediaLocator.Search(destDir, recursive: SelectedConfig.Recursive, cancellationToken: cancellationToken))
                        {
                            filesExtracted = true;
                            _dbContext.FileDetails.Add(file);
                        }
                    }
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                while (filesExtracted && extractionDepth <= SelectedConfig.ExtractionDepth);
            }

            if (SelectedConfig.PerformDeepAnalysis)
            {
                await UpdateProgressIndicator("Analysing Files...", cancellationToken);
                // TODO: Deep File Analysis for media types
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await UpdateProgressIndicator("Calculating Hashes...", cancellationToken);
            var total = await _dbContext.FileDetails
                .CountAsync(f => f.FileType != DataAccessLayer.Models.MultiMediaType.Archive, cancellationToken);
            var current = 0;
            await foreach (var file in _dbContext.FileDetails
                .Where(f => f.FileType != DataAccessLayer.Models.MultiMediaType.Archive)
                .AsAsyncEnumerable())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var filePath = Path.Combine(file.ParentPath, file.FileName);
                await UpdateProgressIndicator($"Calculating Hash {++current} of {total}\nFile: {filePath}", cancellationToken);
                var fileInfo = new FileInfo(filePath);

                using var fileStream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                using var hashStream = new HashStream(fileStream, HashAlgorithmName.MD5, HashAlgorithmName.SHA256, HashAlgorithmName.SHA512);
                using var streamReader = new StreamReader(hashStream);

                _ = await streamReader.ReadToEndAsync(cancellationToken);

                file.MD5_Hash = Convert.ToHexString(hashStream.Hash(HashAlgorithmName.MD5));
                file.SHA256_Hash = Convert.ToHexString(hashStream.Hash(HashAlgorithmName.SHA256));
                file.SHA512_Hash = Convert.ToHexString(hashStream.Hash(HashAlgorithmName.SHA512));
            }
            await _dbContext.SaveChangesAsync(cancellationToken);

            await UpdateProgressIndicator("Saving Results...", cancellationToken);
            await foreach (var file in _dbContext.FileDetails
                .Where(f => f.FileType == DataAccessLayer.Models.MultiMediaType.Image || f.FileType == DataAccessLayer.Models.MultiMediaType.Video)
                .AsAsyncEnumerable())
            {
                file.ShouldExport = true;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);

            await UpdateProgressIndicator("Removing Duplicates...", cancellationToken);
            var md5DupeCount = 0;
            var md5_hashes = await _dbContext.FileDetails
                .Where(f => f.ShouldExport && f.MD5_Hash != null)
                .GroupBy(f => f.MD5_Hash)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);
            await UpdateProgressIndicator($"Removing Duplicates...\nBased on MD5 Hash: {md5_hashes.Count}", cancellationToken);
            foreach (var md5_hash in md5_hashes)
            {
                await foreach (var file in _dbContext.FileDetails
                    .Where(f => f.MD5_Hash == md5_hash)
                    .Skip(1)
                    .AsAsyncEnumerable())
                {
                    file.ShouldExport = false;
                    ++md5DupeCount;
                }
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            var sha256DupeCount = 0;
            var sha256_hashes = await _dbContext.FileDetails
                .Where(f => f.ShouldExport && f.SHA256_Hash != null)
                .GroupBy(f => f.SHA256_Hash)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);
            await UpdateProgressIndicator($"Removing Duplicates...\nBased on SHA256 Hash: {sha256_hashes.Count}", cancellationToken);
            foreach (var sha256_hash in sha256_hashes)
            {
                await foreach (var file in _dbContext.FileDetails
                    .Where(f => f.SHA256_Hash == sha256_hash)
                    .Skip(1)
                    .AsAsyncEnumerable())
                {
                    file.ShouldExport = false;
                    ++sha256DupeCount;
                }
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            var sha512DupeCount = 0;
            var sha512_hashes = await _dbContext.FileDetails
                .Where(f => f.ShouldExport && f.SHA512_Hash != null)
                .GroupBy(f => f.SHA512_Hash)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);
            await UpdateProgressIndicator($"Removing Duplicates...\nBased on SHA512 Hash: {sha512_hashes.Count}", cancellationToken);
            foreach (var sha512_hash in sha512_hashes)
            {
                await foreach (var file in _dbContext.FileDetails
                    .Where(f => f.SHA512_Hash == sha512_hash)
                    .Skip(1)
                    .AsAsyncEnumerable())
                {
                    file.ShouldExport = false;
                    ++sha512DupeCount;
                }
            }
            await _dbContext.SaveChangesAsync(cancellationToken);

            var totalDupeCount = md5DupeCount + sha256DupeCount + sha512DupeCount;
            await UpdateProgressIndicator($"Finalizing...\nTotal Duplicates Found: {totalDupeCount}\nMD5: {md5DupeCount}\nSHA256: {sha256DupeCount}\nSHA512: {sha512DupeCount}", cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            SearchComplete = true;
        }
        catch (TaskCanceledException)
        {
            _messenger.Send(SnackBarMessage.Create("Search cancelled"));
        }
        catch (Exception ex)
        {
            _messenger.Send(SnackBarMessage.Create($"Search failed: {ex.Message}"));
        }
        finally
        {
            await HideProgressIndicator();
        }
    }

    [RelayCommand(CanExecute = nameof(CanMoveToReview))]
    public async Task OnMoveToReview()
    {
        var reload = LoadingResultsCommand.ExecuteAsync(null);
        Transitioner.MoveNextCommand.Execute(null, null);
        await reload;
    }

    public bool CanMoveToReview()
        => SearchComplete;

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
    private bool _isExporting;

    [RelayCommand]
    public async Task OnLoadingResults()
    {
        if (!SearchComplete)
        {
            return;
        }

        await ShowProgressIndicator("Populating Results...");
        DiscoveredFiles.Clear();
        await foreach (var file in _dbContext.FileDetails
            .AsAsyncEnumerable())
        {
            DiscoveredFiles.Add(new MediaFile(file));
        }
        await HideProgressIndicator();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    public static void OnBackToSearch(CancellationToken cancellationToken)
    {
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

    [RelayCommand(IncludeCancelCommand = true)]
    public async Task Finish(CancellationToken cancellationToken)
    {
        if (!SearchComplete)
        {
            return;
        }

        await _dbContext.FileDetails.ExecuteDeleteAsync(cancellationToken);
        if (SelectedConfig?.WorkingDirectory is not null
            && Directory.Exists(SelectedConfig.WorkingDirectory))
        {
            Directory.Delete(SelectedConfig.WorkingDirectory, true);
        }
        SearchComplete = false;
        SelectedConfig = null;
        Transitioner.MoveFirstCommand.Execute(null, null);
    }

    #endregion
}
