using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.Helpers;

using Microsoft.Extensions.Logging;

using SevenZipExtractor;

namespace MediaFinder_v2.Services.Search;

public class SearchStageOneWorker : ReactiveBackgroundWorker<SearchRequest>
{
    private const int SixteenKBytes = 1024 * 16;

    public SearchStageOneWorker(ILogger<SearchStageOneWorker> logger, IMessenger messenger)
        : base(logger, messenger)
    {
    }

    protected override void Execute(SearchRequest inputs, DoWorkEventArgs e)
    {
        SetProgress("Preparing Working Directory...");
        var workingDirectory = Path.Combine(inputs.WorkingDirectory, Guid.NewGuid().ToString());
        Directory.CreateDirectory(workingDirectory);
        ReportProgress(WorkingDirectoryCreated.Creatae(workingDirectory));
        LogDebug($"Working directory created: {workingDirectory}");

        var files = IterateFiles(
            inputs.SourceDirectories,
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
        e.Result = SearchResponse.Create(files.Distinct().ToList());
    }

    private List<string> IterateFiles(
        IEnumerable<string> directories,
        bool recursive,
        bool extractArchive,
        int extractionDepth,
        string workingDirectory)
    {
        var files = new List<string>();
        foreach (var directory in directories.Where(Directory.Exists))
        {
            if (CancellationPending)
            {
                break;
            }

            files.AddRange(IterateFiles(directory, recursive, extractArchive, extractionDepth, workingDirectory));
        }
        return files;
    }

    private List<string> IterateFiles(
        string directory,
        bool recursive,
        bool extractArchive,
        int extractionDepth,
        string workingDirectory)
    {
        var files = new ConcurrentBag<string>();
        var extracted = new ConcurrentBag<string>();

        foreach(var f in Directory.EnumerateFiles(directory, "*", GetEnumerationOptions(recursive)))
        {
            if (CancellationPending)
            {
                break;
            }
            SetProgress($"Iterating files in directory: {directory}");

            var isExtracted = false;
            if (extractArchive && extractionDepth != 0)
            {
                LogDebug("Attempting archive extraction...");
                var extractionPath = Path.Combine(workingDirectory, $"Extracted_{Path.GetFileNameWithoutExtension(f)}");
                if (ExtractArchive(f, extractionPath))
                {
                    LogDebug("Archive extracted to: {extractionPath}", extractionPath);
                    isExtracted = true;
                    extracted.Add(extractionPath);
                }
            }
            if (!isExtracted)
            {
                files.Add(f);
            }
        }

        foreach (var file in IterateFiles(extracted, recursive, extractArchive, extractionDepth - 1, workingDirectory))
        {
            if (CancellationPending)
            {
                break;
            }

            files.Add(file);
        }

        return files.Distinct().ToList();
    }

    private bool ExtractArchive(string filepath, string destinationPath)
    {
        try
        {
            LogDebug("Performing archive detection: {filepath}", filepath);
            using var archive = new ArchiveFile(filepath);
            SetProgress($"Extracting Archive: {filepath}");
            archive.Extract(destinationPath);
            return true;
        }
        catch (SevenZipException ex)
        {
            LogDebug(ex, "Archive detection failed for {filepath}", filepath);
            return false;
        }
    }
    private static EnumerationOptions GetEnumerationOptions(bool recursive)
        => new()
        {
            BufferSize = SixteenKBytes,
            IgnoreInaccessible = true,
            RecurseSubdirectories = recursive,
        };
}
