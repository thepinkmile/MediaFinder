namespace MediaFinder.Helpers;

public static class DirectoryInfoExtensions
{
    public const int SixteenKBytes = 1024 * 16;

    // a list of known directories that cause issues with downloading files from the cloud
    public static readonly List<string> DirectoryExclusions = [
        "OneDrive",
        "iCloud Drive",
        "Creative Cloud Files",
        "Album Artwork"
    ];

    public static readonly List<string> ExtensionExclusions = [
        ".lnk",
        ".exe",
        ".dll",
        ".app"
    ];

    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        BufferSize = SixteenKBytes,
        IgnoreInaccessible = true,
        RecurseSubdirectories = false,
    };

    public static IEnumerable<DirectoryInfo> EnumerateSubDirectories(this DirectoryInfo directoryInfo)
        => directoryInfo
            .EnumerateDirectories("*", EnumerationOptions)
            .Where(subDir => !DirectoryExclusions.Contains(subDir.Name, StringComparer.InvariantCultureIgnoreCase));

    public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo directoryInfo)
        => directoryInfo
            .EnumerateFiles("*", EnumerationOptions)
            .Where(file => !ExtensionExclusions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase));

    public static ulong GetDirectorySize(this DirectoryInfo directory)
        => directory.Exists
            ? Convert.ToUInt64(directory.EnumerateSubDirectories().Sum(d => (decimal)d.GetDirectorySize()))
                + Convert.ToUInt64(directory.EnumerateFiles().Sum(f => (decimal)f.GetFileSize()))
            : 0UL;

    public static ulong GetFileSize(this FileInfo file)
        => Convert.ToUInt64(file.Length);
}
