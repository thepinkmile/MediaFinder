using System.Collections.Concurrent;
using System.ComponentModel;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder.Helpers;
using MediaFinder.Logging;
using MediaFinder.Messages;

using Microsoft.Extensions.Logging;

using SevenZipExtractor;

namespace MediaFinder.Services.Search;

public class SearchStageOneWorker(
    ILogger<SearchStageOneWorker> logger,
    IMessenger messenger)
    : ReactiveBackgroundWorker<SearchRequest>(logger, messenger)
{
    private readonly IMessenger _messenger = messenger;

    protected override void Execute(SearchRequest inputs, DoWorkEventArgs e)
    {
        SetProgress("Preparing Working Directory...");
        var workingDirectory = Path.Combine(inputs.WorkingDirectory, Guid.NewGuid().ToString());
        Directory.CreateDirectory(workingDirectory);
        ReportProgress(WorkingDirectoryCreated.Create(workingDirectory));
        _logger.CreatedWorkingDirectory(workingDirectory);

        var files = IterateDirectories(
            inputs.SourceDirectories.Select(x => new DirectoryInfo(x)),
            inputs.Recursive,
            inputs.ExtractArchives,
            inputs.ExtractionDepth ?? 0,
            workingDirectory);
        if (CancellationPending)
        {
            e.Cancel = true;
            return;
        }

        SetProgress("Finalising Search Results...");
        e.Result = SearchResponse.Create(
            files
                .Distinct()
                .Select(f => f.FullName)
                .ToList());
    }

    protected override void UpdateProgress(object? sender, ProgressChangedEventArgs e)
    {
        switch (e.UserState)
        {
            case WorkingDirectoryCreated workingDirectoryCreatedMessage: _messenger.Send(workingDirectoryCreatedMessage); break;
            case FileExtracted extractedMessage: _messenger.Send(extractedMessage); break;
        }
        base.UpdateProgress(sender, e);
    }

    private List<FileInfo> IterateDirectories(
        IEnumerable<DirectoryInfo> directories,
        bool recursive,
        bool extractArchive,
        int extractionDepth,
        string workingDirectory)
    {
        var files = new List<FileInfo>();
        foreach (var directory in directories.Where(d => d.Exists))
        {
            if (CancellationPending)
            {
                break;
            }

            ReportProgress($"Iterating files in directory: {directory.FullName}");
            _logger.IteratingDirectory(directory.FullName);

            files.AddRange(IterateFiles(directory, extractArchive, extractionDepth, workingDirectory));

            if (recursive)
            {
                var subDirectories = directory.EnumerateSubDirectories();
                files.AddRange(IterateDirectories(subDirectories, recursive, extractArchive, extractionDepth, workingDirectory));
            }
        }
        return files;
    }

    private List<FileInfo> IterateFiles(
        DirectoryInfo directory,
        bool extractArchive,
        int extractionDepth,
        string workingDirectory)
    {
        var files = new ConcurrentBag<FileInfo>();
        var extracted = new ConcurrentBag<DirectoryInfo>();

        var enumeratedFiles = directory.EnumerateFiles().ToList();
        var index = 0;
        foreach (var f in enumeratedFiles)
        {
            if (CancellationPending)
            {
                break;
            }

            ++index;
            var percent = (int)Math.Round((index / enumeratedFiles.Count) * (decimal)100, 0, MidpointRounding.AwayFromZero);
            ReportProgress(percent, $"Iterating files in directory: {directory.FullName}\nFile {index} of {enumeratedFiles.Count}");

            var isExtracted = false;
            if (extractArchive && extractionDepth != 0)
            {
                var extractionDirectory = GetExtractionPath(workingDirectory, f.Name);
                if (ExtractArchive(f, extractionDirectory))
                {
                    extractionDirectory.Refresh();
                    isExtracted = true;
                    extracted.Add(extractionDirectory);
                }
            }

            // don't add file to list if it was an extracted archive
            if (!isExtracted)
            {
                files.Add(f);
            }
        }

        foreach (var file in IterateDirectories(extracted, true, extractArchive, extractionDepth - 1, workingDirectory))
        {
            if (CancellationPending)
            {
                break;
            }

            files.Add(file);
        }

        return [..files];
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

    private static readonly string[] KnownNonArchiveExtensions = [".ipa", ".ibooks", ".epub"];

    private bool ExtractArchive(FileInfo filepath, DirectoryInfo destinationPath)
    {
        var extension = filepath.Extension.ToLowerInvariant();
        if (KnownNonArchiveExtensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
        {
            _logger.KnownNonArchive(filepath.FullName);
            return false;
        }

        try
        {
            _logger.PerformingArchiveDetection(filepath.FullName);
            using var archive = new ArchiveFile(filepath.FullName);
            ReportProgress($"Extracting Archive: {filepath.FullName}");
            _logger.PerformingArchiveExtraction(filepath.FullName);
            archive.Extract(destinationPath.FullName, true);
            _logger.ArchiveExtracted(filepath.FullName);
            ReportProgress(FileExtracted.Create(filepath.FullName));
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
