using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Services.Archive;

public class ArchiveExtractor : ArchiveToolBase
{
    public ArchiveExtractor(ILogger<ArchiveExtractor> logger)
        : base(logger)
    {
    }

    public bool Extract(string filepath, string destinationPath, bool overwrite = false)
    {
        if (!TryLoadArchive(filepath, out var archive))
            return false;

        _logger.LogDebug("Extracting Archive '{filepath}'", filepath);
        archive!.Extract(destinationPath, overwrite);
        archive.Dispose();
        return true;
    }
}
