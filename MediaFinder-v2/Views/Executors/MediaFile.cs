using CommunityToolkit.Mvvm.ComponentModel;

using MediaFinder_v2.DataAccessLayer.Models;

namespace MediaFinder_v2.Views.Executors;

public partial class MediaFile : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _parentPath;

    [ObservableProperty]
    private string _fileName;

    [ObservableProperty]
    private long _fileSize;

    [ObservableProperty]
    private bool _shouldExport = true;

    [ObservableProperty]
    private bool _isImage;

    [ObservableProperty]
    private bool _isVideo;

    [ObservableProperty]
    private bool _isArchive;

    [ObservableProperty]
    private string? _md5Hash;

    [ObservableProperty]
    private string? _sha256Hash;

    [ObservableProperty]
    private string? _sha512Hash;

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
        Md5Hash = file.MD5_Hash;
        Sha256Hash = file.SHA256_Hash;
        Sha512Hash = file.SHA512_Hash;
    }
}
