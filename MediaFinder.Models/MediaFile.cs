using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;

using MediaFinder.DataAccessLayer.Models;

namespace MediaFinder.Models;

public partial class MediaFile : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _parentPath;

    [ObservableProperty]
    private string _fileName;

    [ObservableProperty]
    private string _filePath;

    [ObservableProperty]
    private long _fileSize;

    [ObservableProperty]
    private DateTimeOffset _dateCreated;

    [ObservableProperty]
    private bool _shouldExport = true;

    [ObservableProperty]
    private MultiMediaType _multiMediaType;

    public int? Width => Properties.TryGetValue("Width", out var propValue) && int.TryParse(propValue, out var width) ? width : null;

    public int? Height => Properties.TryGetValue("Height", out var propValue) && int.TryParse(propValue, out var width) ? width : null;

    [ObservableProperty]
    private Dictionary<string, string> _properties;

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
        FilePath = Path.Combine(file.ParentPath, file.FileName);
        FileSize = file.FileSize;
        DateCreated = file.Created;
        ShouldExport = file.ShouldExport;
        MultiMediaType = file.FileType;
        Md5Hash = file.MD5_Hash;
        Sha256Hash = file.SHA256_Hash;
        Sha512Hash = file.SHA512_Hash;

        // add properties with case insensitive key search
        Properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var prop in file.FileProperties)
        {
            Properties.TryAdd(prop.Name, prop.Value);
        }
    }

    public static MediaFile Create(FileDetails file)
        => new(file);
}
