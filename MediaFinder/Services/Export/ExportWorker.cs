using System.ComponentModel;
using System.Globalization;
using System.IO;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Helpers;
using MediaFinder_v2.Logging;

using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Services.Export;

public class ExportWorker : ReactiveBackgroundWorker<ExportRequest>
{
    public ExportWorker(ILogger<ExportWorker> logger, IMessenger messenger)
        : base(logger, messenger)
    {   
    }

    protected override void Execute(ExportRequest inputs, DoWorkEventArgs e)
    {
        SetProgress("Exporting Files...");

        var cts = new CancellationTokenSource();
        var exportTasks = new List<Task>(inputs.Files.Count);
        foreach(var file in inputs.Files)
        {
            if (CancellationPending)
            {
                break;
            }

            var originalFilePath = Path.Combine(file.ParentPath, file.FileName);

            var path = GetDestinationPath(file, inputs.ExportDirectory, inputs.Type);
            var filename = GetDestinationFileName(file, inputs.RenameFiles, inputs.Type);

            var fullpath = Path.Combine(path, filename);

            EnsureDestinationDirectoryExists(path);

            exportTasks.Add(CopyFile(originalFilePath, fullpath, cts.Token));
        }
        while (!exportTasks.All(t => t.IsCompleted || t.IsCompletedSuccessfully || t.IsFaulted || t.IsCanceled)
                && !cts.IsCancellationRequested)
        {
            Thread.Sleep(500);
            if (CancellationPending)
            {
                cts.Cancel();
            }
        }
        Task.WaitAll(exportTasks.ToArray());

        if (CancellationPending)
        {
            e.Cancel = true;
            return;
        }

        e.Result = true;
    }

    private void EnsureDestinationDirectoryExists(string path)
    {
        var parentPath = Path.GetDirectoryName(path);
        if (Directory.Exists(path) || parentPath is null)
            return;

        EnsureDestinationDirectoryExists(parentPath!);
        _logger.CreatingDirectory(path);
        Directory.CreateDirectory(path);
    }

    private static string GetDestinationFileName(FileDetails file, bool renameFiles, ExportType type)
    {
        if (!renameFiles)
            return file.FileName;

        var filename = type == ExportType.OriginalPath
            ? file.FileName
            : $"{file.MD5_Hash}__{file.FileName}";
        var expectedExtension = file.GetExpectedExtension();
        return file.GetExtension().Equals(expectedExtension, StringComparison.InvariantCultureIgnoreCase)
            ? filename
            : Path.GetFileNameWithoutExtension(filename) + expectedExtension;
    }

    private static string GetDestinationPath(FileDetails file, string exportDirectory, ExportType type)
    {
        var pathFunc = GetPathFunc(type);
        var relativePath = pathFunc(file);
        var fullPath = Path.Combine(exportDirectory, relativePath);
        return fullPath;
    }

    private static Func<FileDetails, string> GetPathFunc(ExportType type)
    {
        switch (type)
        {
            case ExportType.ByDateCreated: return file => Path.Combine(
                file.Created.Year.ToString(CultureInfo.InvariantCulture),
                CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(file.Created.Month),
                file.Created.Day.ToString(CultureInfo.InvariantCulture)
                );
            case ExportType.ByChecksum: return file => Path.Combine(
                file.MD5_Hash![0..2],
                file.MD5_Hash![2..4],
                file.MD5_Hash![4..6]
                ); ;
            case ExportType.Flat: return file => string.Empty;
            default: return file => Path.GetDirectoryName(file.RelativePath)!.TrimStart('\\');
        }
    }

    private Task CopyFile(string source, string destination, CancellationToken cancellationToken = default)
    {
        if (File.Exists(source))
        {
            _logger.ExportingFile(source, destination);
            using var inputStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            using var outputStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 1024);

            var buffer = new byte[1024];
            int bytesRead = 0;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    outputStream.Write(buffer, 0, bytesRead);
                }
            }
            while (bytesRead > 0);
        }
        return Task.CompletedTask;
    }
}
