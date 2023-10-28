using System.Collections;
using System.IO;

using MetadataExtractor;

namespace MediaFinder_v2.Helpers;

public static class DirectoryInfoExtensions
{
    public const int SixteenKBytes = 1024 * 16;

    // a list of known directories that cuase issues with dowloading files from the cloud
    public static readonly List<string> DirectoryExclusions = new()
    {
        "OneDrive",
        "iCloud Drive",
        "Creative Cloud Files"
    };

    private static readonly EnumerationOptions _enumerationOptions = new EnumerationOptions
    {
        BufferSize = SixteenKBytes,
        IgnoreInaccessible = true,
        RecurseSubdirectories = false,
    };

    public static IEnumerable<DirectoryInfo> EnumerateSubDirectories(this DirectoryInfo directoryInfo)
        => directoryInfo
            .EnumerateDirectories("*", _enumerationOptions)
            .Where(subDir => !DirectoryExclusions.Contains(subDir.Name, StringComparer.InvariantCultureIgnoreCase));

    public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo directoryInfo)
        => directoryInfo.EnumerateFiles("*", _enumerationOptions);

    public static ulong GetDirectorySize(this DirectoryInfo directory)
    {
        var size = 0UL;
        foreach (var dir in directory.EnumerateDirectories("*", new EnumerationOptions
        {
            BufferSize = SixteenKBytes,
            IgnoreInaccessible = true,
            RecurseSubdirectories = false,
        }))
        {
            size += GetDirectorySize(dir);
        }

        foreach (var file in directory.EnumerateFiles("*", new EnumerationOptions
        {
            BufferSize = SixteenKBytes,
            IgnoreInaccessible = true,
            RecurseSubdirectories = false,
        }))
        {
            size += GetFileSize(file);
        }

        return size;
    }

    public static ulong GetFileSize(this FileInfo file)
        => Convert.ToUInt64(file.Length);
}
