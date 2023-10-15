using System.IO;

using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Services.Search;

namespace MediaFinder_v2.Services.Export;

public static class FileDetailsExtensions
{
    public static string GetExtension(this FileDetails file)
        => file.FileProperties.FirstOrDefault(fp => fp.Name == SearchStageTwoWorker.EXTENSION_DETAIL)?.Value
            ?? Path.GetExtension(file.FileName);

    public static string GetExpectedExtension(this FileDetails file)
        => file.FileProperties.FirstOrDefault(fp => fp.Name == SearchStageTwoWorker.EXPECTED_EXTENSION_DETAIL)?.Value
            ?? Path.GetExtension(file.FileName);
}
