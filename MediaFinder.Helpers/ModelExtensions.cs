using MediaFinder.DataAccessLayer.Models;
using MediaFinder.Models;

namespace MediaFinder.Helpers;

public static class ModelExtensions
{
    public static DiscoveryOptions ToDiscoveryOptions(this SearchSettings item)
        => new()
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Directories = item.Directories.Select(x => x.Path).ToList(),
            Recursive = item.Recursive,
            ExtractArchives = item.ExtractArchives,
            ExtractionDepth = item.ExtractionDepth,
            PerformDeepAnalysis = item.PerformDeepAnalysis,
            MinImageHeight = item.MinImageHeight,
            MinImageWidth = item.MinImageWidth,
            MinVideoHeight = item.MinVideoHeight,
            MinVideoWidth = item.MinVideoWidth
        };

    public static MediaFile ToMediaFile(this FileDetails file)
        => new()
        {
            Id = file.Id,
            ParentPath = file.ParentPath,
            RelativePath = file.RelativePath,
            FileName = file.FileName,
            FilePath = Path.Combine(file.ParentPath, file.FileName),
            FileSize = file.FileSize,
            DateCreated = file.Created,
            ShouldExport = file.ShouldExport,
            MultiMediaType = file.FileType,
            Md5Hash = file.MD5_Hash,
            Sha256Hash = file.SHA256_Hash,
            Sha512Hash = file.SHA512_Hash,
            Properties = file.FileProperties.ToDictionary(x => x.Name, x => x.Value, StringComparer.InvariantCultureIgnoreCase)
        };

    public static FileDetails ToFileDetails(this MediaFile file)
        => new()
        {
            Id = file.Id,
            ParentPath = file.ParentPath!,
            RelativePath = file.RelativePath!,
            FileName = file.FileName!,
            FileSize = file.FileSize,
            Created = file.DateCreated,
            ShouldExport = file.ShouldExport,
            FileType = file.MultiMediaType,
            MD5_Hash = file.Md5Hash,
            SHA256_Hash = file.Sha256Hash,
            SHA512_Hash = file.Sha512Hash,
            FileProperties = file.Properties?
                .Select(x => new FileProperty() { Name = x.Key, Value = x.Value })
                .ToList()
                ?? []
        };
}
