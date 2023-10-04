using System.IO;

namespace MediaFinder_v2.Services;

public record EnumerateDirectoryState(string Path, string Pattern, SearchOption SearchOption)
{
    public static EnumerateDirectoryState Create(string path, string pattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        => new(path, pattern, searchOption);
}
