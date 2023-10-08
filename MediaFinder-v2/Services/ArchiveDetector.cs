using MediaFinder_v2.DataAccessLayer.Models;

using SevenZipExtractor;

namespace MediaFinder_v2.Services;

public class ArchiveDetector : IMediaDetector
{
    public MultiMediaType MediaType => MultiMediaType.Archive;

    public IDictionary<string, string> GetMediaProperties(string filepath)
    {
        if (!IsPositiveDetection(filepath, true))
            return new Dictionary<string, string>();

        using var archive = new ArchiveFile(filepath);

        var result = new Dictionary<string, string>
        {
            { "EntriesCount", archive.Entries.Count.ToString() }
        };

        var i = 0;
        foreach (var entry in archive.Entries)
        {
            result.Add($"Entry{i}_FileNamae", entry.FileName);
            result.Add($"Entry{i}_Size", entry.Size.ToString());
            result.Add($"Entry{i}_PackedSize", entry.PackedSize.ToString());
            result.Add($"Entry{i}_CreationTime", entry.CreationTime.ToString("O"));
            result.Add($"Entry{i}_Comment", entry.Comment);

            ++i;
        }

        return result;
    }

    public bool IsPositiveDetection(string filepath, bool performDeepAnalysis)
    {
        bool result = false;
        try
        {
            using var archive = new ArchiveFile(filepath);
            result = true;
        }
        catch(SevenZipException)
        {
            // do nothing
        }
        return result;
    }
}
