using MediaFinder.DataAccessLayer.Models;
using MediaFinder.Helpers;
using MediaFinder.Models;

namespace MediaFinder.Messages;

public class ExportRequest : ReactiveBackgroundWorkerContextBase
{
    private ExportRequest(
        object progressToken,
        ICollection<FileDetails> files,
        string exportDirectory,
        ExportType type,
        bool renameFiles)
        : base(progressToken)
    {
        Files = files;
        ExportDirectory = exportDirectory;
        Type = type;
        RenameFiles = renameFiles;
    }

    public ICollection<FileDetails> Files { get; }

    public string ExportDirectory { get; }

    public ExportType Type { get; }

    public bool RenameFiles { get; }

    public static ExportRequest Create(object progressToken, ICollection<FileDetails> files,
        string exportDirectory, ExportType type, bool renameFiles)
        => new(progressToken, files, exportDirectory, type, renameFiles);
}
