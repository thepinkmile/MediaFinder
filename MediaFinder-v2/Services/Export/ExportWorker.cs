using System.ComponentModel;
using System.Globalization;
using System.IO;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Helpers;

using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Services.Export;

public class ExportWorker : ReactiveBackgroundWorker
{
    public ExportWorker(ILogger<ExportWorker> logger, IMessenger messenger)
        : base(logger, messenger)
    {   
    }

    protected override void Execute(object? sender, DoWorkEventArgs e)
    {
        if (e.Argument is not ExportRequest inputs)
        {
            throw new InvalidOperationException("Stage called with invalid arguments.");
        }

        SetProgress("Exporting Files...");

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

            exportTasks.Add(Task.Factory.StartNew(() =>
            {
                if (!File.Exists(originalFilePath))
                    return;

                using var inputStream = new FileStream(originalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
                using var outputStream = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.None, 1024);

                var buffer = new byte[1024];
                int bytesRead = 0;
                do
                {
                    if (CancellationPending)
                    {
                        return;
                    }

                    bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                }
                while (bytesRead > 0);
            }));
        }
        Task.WaitAll(exportTasks.ToArray());

        if (CancellationPending)
        {
            e.Cancel = true;
            return;
        }

        e.Result = true;
    }

    private static void EnsureDestinationDirectoryExists(string path)
    {
        var parentPath = Path.GetDirectoryName(path);
        if (Directory.Exists(path) || parentPath is not null)
            return;

        EnsureDestinationDirectoryExists(parentPath!);
        Directory.CreateDirectory(path);
    }

    private static string GetDestinationFileName(FileDetails file, bool renameFiles, ExportType type)
    {
        if (!renameFiles)
            return file.FileName;

        var filename = $"{file.MD5_Hash}__{file.FileName}";
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
                file.Created.Month.ToString(CultureInfo.InvariantCulture),
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
}
