using MediaFinder_v2.DataAccessLayer.Models;

namespace MediaFinder_v2.Services.Export;

public record ExportRequest(ICollection<FileDetails> Files, string ExportDirectory, ExportType Type, bool RenameFiles)
{
    public static ExportRequest Create(ICollection<FileDetails> files, string exportDirectory, ExportType type, bool renameFiles)
        => new(files, exportDirectory, type, renameFiles);
}
