using MediaFinder_v2.DataAccessLayer.Models;

namespace MediaFinder_v2.Views.Executors;

public class MediaFile
{
    public int Id { get; set; }

    public string ParentPath { get; set; }

    public string FileName { get; set; }

    public long FileSize { get; set; }

    public bool ShouldExport { get; set; } = true;

    public bool IsImage { get; set; }

    public bool IsVideo { get; set; }

    public bool IsArchive { get; set; }

    public MediaFile(FileDetails file)
    {
        Id = file.Id;
        ParentPath = file.ParentPath;
        FileName = file.FileName;
        FileSize = file.FileSize;
        ShouldExport = file.ShouldExport;
        IsImage = file.FileType == MultiMediaType.Image;
        IsVideo = file.FileType == MultiMediaType.Video;
        IsArchive = file.FileType == MultiMediaType.Archive;
    }
}
