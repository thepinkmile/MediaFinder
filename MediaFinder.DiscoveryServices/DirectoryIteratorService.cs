using MediaFinder.Helpers;
using System.Collections.Concurrent;

using MediaFinder.Logging;
using MediaFinder.Messages;

using Microsoft.Extensions.Logging;

using SevenZipExtractor;
using Microsoft.Extensions.Options;
using MediaFinder.DataAccessLayer;

namespace MediaFinder.DiscoveryServices;

public class DirectoryIteratorService
{
    private readonly ILogger<DirectoryIteratorService> _logger;
    private readonly MediaFinderDbContext _dbContext;
    private readonly KnownFalseArchiveExtensions _knownNonArchiveExtensions;

    public DirectoryIteratorService(
        ILogger<DirectoryIteratorService> logger,
        MediaFinderDbContext dbContext,
        IOptionsMonitor<KnownFalseArchiveExtensions> knownFalseArchiveExtensionsConfigMonitor)
    {
        _logger = logger;
        _dbContext = dbContext;
        _knownNonArchiveExtensions = knownFalseArchiveExtensionsConfigMonitor.CurrentValue;
    }

    public async Task RunAsync(SearchRequest inputs, IProgress<object> progressUpdate, CancellationToken cancellationToken = default)
    {
        progressUpdate.Report("Preparing Working Directory...");
        var workingDirectory = Path.Combine(inputs.WorkingDirectory, Guid.NewGuid().ToString());
        Directory.CreateDirectory(workingDirectory);
        progressUpdate.Report(WorkingDirectoryCreated.Create(workingDirectory));
        _logger.CreatedWorkingDirectory(workingDirectory);

        var files = await IterateDirectories(
            inputs.SourceDirectories.Select(x => new DirectoryInfo(x)),
            inputs.Recursive,
            inputs.ExtractArchives,
            inputs.ExtractionDepth ?? 0,
            workingDirectory,
            progressUpdate,
            cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var filteredFiles = files
                .Distinct()
                .Select(f => f.FullName)
                .ToList();

        // TODO: store in database
        await Task.Delay(100, cancellationToken);
    }

    private async Task<List<FileInfo>> IterateDirectories(
        IEnumerable<DirectoryInfo> directories,
        bool recursive,
        bool extractArchive,
        int extractionDepth,
        string workingDirectory,
        IProgress<object> progressUpdate, 
        CancellationToken cancellationToken = default)
    {
        var files = new List<FileInfo>();
        foreach (var directory in directories.Where(d => d.Exists))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            progressUpdate!.Report($"Iterating files in directory: {directory.FullName}");
            _logger.IteratingDirectory(directory.FullName);

            files.AddRange(await IterateFiles(directory, extractArchive, extractionDepth, workingDirectory, progressUpdate, cancellationToken));

            if (recursive)
            {
                var subDirectories = directory.EnumerateSubDirectories();
                files.AddRange(await IterateDirectories(subDirectories, recursive, extractArchive, extractionDepth, workingDirectory, progressUpdate, cancellationToken));
            }
        }
        return files;
    }

    private async Task<List<FileInfo>> IterateFiles(
        DirectoryInfo directory,
        bool extractArchive,
        int extractionDepth,
        string workingDirectory,
        IProgress<object> progressUpdate,
        CancellationToken cancellationToken = default)
    {
        var files = new ConcurrentBag<FileInfo>();
        var extracted = new ConcurrentBag<DirectoryInfo>();

        var enumeratedFiles = directory.EnumerateFiles().ToList();
        var index = 0;
        foreach (var f in enumeratedFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            ++index;
            progressUpdate!.Report($"Iterating files in directory: {directory.FullName}\nFile {index} of {enumeratedFiles.Count}");

            var isExtracted = false;
            if (extractArchive && extractionDepth != 0)
            {
                var extractionDirectory = GetExtractionPath(workingDirectory, f.Name);
                if (ExtractArchive(f, extractionDirectory, progressUpdate))
                {
                    extractionDirectory.Refresh();
                    isExtracted = true;
                    extracted.Add(extractionDirectory);
                }
            }

            // TODO: Do we actually want to keep this as it may be useful for advanced filters to link files to parent archive details???
            // don't add file to list if it was an extracted archive
            if (!isExtracted)
            {
                files.Add(f);
            }
        }

        foreach (var file in await IterateDirectories(extracted, true, extractArchive, extractionDepth - 1, workingDirectory, progressUpdate, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            files.Add(file);
        }

        return [.. files];
    }

    private DirectoryInfo GetExtractionPath(string rootDirectory, string archiveName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(archiveName);
        var extractionPath = new DirectoryInfo(
            Path.Combine(rootDirectory, $"Extracted_{nameWithoutExtension}")
            );
        var index = 0;
        while (extractionPath.Exists)
        {
            _logger.ExtractionPathExists(extractionPath.FullName);
            extractionPath = new DirectoryInfo(
                Path.Combine(rootDirectory, $"Extracted_{nameWithoutExtension}({++index})")
                );
        }

        return extractionPath;
    }

    private bool ExtractArchive(FileInfo filepath, DirectoryInfo destinationPath, IProgress<object> progressUpdate)
    {
        var extension = filepath.Extension.ToLowerInvariant();
        if (_knownNonArchiveExtensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
        {
            _logger.KnownNonArchive(filepath.FullName);
            return false;
        }

        try
        {
            _logger.PerformingArchiveDetection(filepath.FullName);
            using var archive = new ArchiveFile(filepath.FullName);
            progressUpdate!.Report($"Extracting Archive: {filepath.FullName}");
            _logger.PerformingArchiveExtraction(filepath.FullName);
            archive.Extract(destinationPath.FullName, true);
            _logger.ArchiveExtracted(filepath.FullName);
            progressUpdate!.Report(FileExtracted.Create(filepath.FullName));
            return true;
        }
        catch (SevenZipException ex)
        {
            _logger.ArchiveExtractionFailed(ex, filepath.FullName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.ArchiveExtractionFailed(ex, filepath.FullName);
            return false;
        }
    }
}
