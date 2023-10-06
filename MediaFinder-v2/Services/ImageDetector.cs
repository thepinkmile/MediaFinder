//using System.Drawing;

using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using MediaFinder_v2.DataAccessLayer.Models;

using MetadataExtractor;
using MetadataExtractor.Formats.FileType;

namespace MediaFinder_v2.Services;

public class ImageDetector : IMediaDetector
{
    private static readonly string[] KnownExtensions = new[] { ".bmp", ".jpg", ".jpeg", ".jfif", ".png", ".tif", ".tiff", ".gif", ".svg" };
    private static readonly Regex PixelsPropertyParser = new Regex("(?<pixels>(\\d+)) pixels", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    public MultiMediaType MediaType => MultiMediaType.Image;

    public IDictionary<string, string> GetMediaProperties(string filepath)
    {
        if (!IsPositiveDetection(filepath, true))
            return new Dictionary<string, string>();
        using var fileStream = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        var img = ImageMetadataReader.ReadMetadata(fileStream);

        var result = new Dictionary<string, string>();
        foreach (var directory in img)
        {
            foreach (var tag in directory.Tags)
            {
                if (tag.Description is not null)
                {
                    result.Add($"{directory.Name}_{tag.Name}", tag.Description);
                }
            }
        }

        if (result.ContainsKey("File Type_Expected File Name Extension"))
        {
            result.Add("ExpectedExtension", result["File Type_Expected File Name Extension"]);
        }

        var widthProperty = result.Keys.FirstOrDefault(k => k.Contains("Image Width"));
        if (widthProperty is not null)
        {
            var match = PixelsPropertyParser.Match(result[widthProperty]);
            var width = match.Groups["pixels"].Value;
            result.Add("Width", width);
        }

        var heightProperty = result.Keys.FirstOrDefault(k => k.Contains("Image Height"));
        if (heightProperty is not null)
        {
            var match = PixelsPropertyParser.Match(result[heightProperty]);
            var height = match.Groups["pixels"].Value;
            result.Add("Height", height);
        }

        var dateProperty = result.Keys.FirstOrDefault(k => k.Contains("Date/Time"));
        if (dateProperty is not null && DateTime.TryParseExact(result[dateProperty], "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var tmp))
        {
            result.Add("CreatedDate", tmp.ToString("O"));
        }

        return result;
    }

    public bool IsPositiveDetection(string filepath, bool performDeepAnalysis)
    {
        var result = KnownExtensions.Contains(Path.GetExtension(filepath), StringComparer.InvariantCultureIgnoreCase);
        if (performDeepAnalysis)
        {
            try
            {
                using var fileStream = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                var img = ImageMetadataReader.ReadMetadata(fileStream);
                result = true;
            }
            catch
            {
                result = false;
            }
        }
        return result;
    }
}
