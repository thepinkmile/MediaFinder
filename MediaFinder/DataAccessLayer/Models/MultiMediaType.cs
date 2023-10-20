namespace MediaFinder_v2.DataAccessLayer.Models;

public enum MultiMediaType
{
    Unknown,

    Image,

    Video
}

public static class MultiMediaTypeHelpers
{
    public static MultiMediaType ToMultiMediaType(this string? type)
    {
        return type?.ToUpperInvariant()?.Trim() switch
        {
            "IMAGE" => MultiMediaType.Image,
            "VIDEO" => MultiMediaType.Video,
            _ => MultiMediaType.Unknown,
        };
    }
}