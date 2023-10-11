using Microsoft.Extensions.Logging;

using SevenZipExtractor;

namespace MediaFinder_v2.Services.Archive;

public abstract class ArchiveToolBase
{
    protected readonly ILogger _logger;

    protected ArchiveToolBase(ILogger logger)
    {
        _logger = logger;
    }

    protected bool TryLoadArchive(string filepath, out ArchiveFile? file)
    {
        try
        {
            file = new ArchiveFile(filepath);
            _logger.LogDebug("Archive Info retrieved for '{filepath}'", filepath);
        }
        catch (SevenZipException ex)
        {
            file = null;
            _logger.LogDebug(ex, "Archive detection failed for {filepath}", filepath);
        }
        return file is not null;
    }
}
