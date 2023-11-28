using CommunityToolkit.Mvvm.ComponentModel;

namespace MediaFinder.Models;

public partial class MediaFile : ObservableObject
{
    public int Id { get; init; }

    [ObservableProperty]
    private string? _parentPath;

    [ObservableProperty]
    private string? _relativePath;

    [ObservableProperty]
    private string? _fileName;

    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private long _fileSize;

    [ObservableProperty]
    private DateTimeOffset _dateCreated;

    [ObservableProperty]
    private bool _shouldExport = true;

    [ObservableProperty]
    private MultiMediaType _multiMediaType;

    public int? Width =>
        Properties is { } &&
        Properties.TryGetValue("Width", out var propValue) &&
        int.TryParse(propValue, out var width)
        ? width
        : null;

    public int? Height =>
        Properties is { } &&
        Properties.TryGetValue("Height", out var propValue) &&
        int.TryParse(propValue, out var width)
        ? width
        : null;

    [ObservableProperty]
    private bool _fromArchive;

    [ObservableProperty]
    private string? _parentArchive;

    [ObservableProperty]
    private Dictionary<string, string>? _properties;

    [ObservableProperty]
    private string? _md5Hash;

    [ObservableProperty]
    private string? _sha256Hash;

    [ObservableProperty]
    private string? _sha512Hash;
}
