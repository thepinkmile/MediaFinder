namespace MediaFinder_v2.DataAccessLayer.Models;

public enum MultiMediaType
{
    Unknown,

    Image,

    Video,

    Archive
}

public static class MultiMediaTypeHelpers
{
    public static MultiMediaType ToMultiMediaType(this string? type)
    {
        return type switch
        {
            "image" => MultiMediaType.Image,
            "video" => MultiMediaType.Video,
            _ => MultiMediaType.Unknown,
        };
    }
}