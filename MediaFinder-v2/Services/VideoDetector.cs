using System.Globalization;
using System.IO;

using MediaFinder_v2.DataAccessLayer.Models;

using NReco.VideoInfo;

namespace MediaFinder_v2.Services;

public class VideoDetector : IMediaDetector
{
    private static readonly FFProbe FFProbe = new();
    private static readonly string[] KnownExtensions = new[] { "webm", "mkv", "flv", "vob", "ogv", "ogg", "rrc", "gifv", "mng", "mov", "avi", "qt", "wmv", "yuv", "rm", "asf", "amv", "mp4", "m4p", "m4v", "mpg", "mp2", "mpeg", "mpe", "mpv", "m4v", "svi", "3gp", "3g2", "mxf", "roq", "nsv", "flv", "f4v", "f4p", "f4a", "f4b", "mod" };

    public MultiMediaType MediaType => MultiMediaType.Video;

    public IDictionary<string, string> GetMediaProperties(string filepath)
    {
        if (!IsPositiveDetection(filepath, true))
            return new Dictionary<string, string>();

        var videoInfo = FFProbe.GetMediaInfo(filepath);
        var result = new Dictionary<string, string>
        {
            { "FormatLongName", videoInfo.FormatLongName },
            { "FormatName", videoInfo.FormatName },
            { "Duration", videoInfo.Duration.ToString() }
        };

        foreach (var tag in videoInfo.FormatTags)
        {
            result.Add($"FormatTag_{tag.Key}", tag.Value);
        }

        var sizeDetected = false;
        foreach (var stream in videoInfo.Streams)
        {
            result.Add($"Stream_{stream.Index}_CodecType", stream.CodecType);
            result.Add($"Stream_{stream.Index}_CodecName", stream.CodecName);
            result.Add($"Stream_{stream.Index}_CodecLongNamae", stream.CodecLongName);
            result.Add($"Stream_{stream.Index}_FrameRate", stream.FrameRate.ToString());
            result.Add($"Stream_{stream.Index}_Height", stream.Height.ToString());
            result.Add($"Stream_{stream.Index}_Width", stream.Width.ToString());
            result.Add($"Stream_{stream.Index}_PixelFormat", stream.PixelFormat);
            foreach (var tag in stream.Tags)
            {
                result.Add($"Stream_{stream.Index}_Tag_{tag.Key}", tag.Value);
            }

            if (!sizeDetected && stream.CodecType == "video")
            {
                result.Add("Width", stream.Height.ToString());
                result.Add("Height", stream.Width.ToString());
                sizeDetected = true;
            }
        }

        if (result.ContainsKey("File Type_Expected File Name Extension"))
        {
            result.Add("ExpectedExtension", videoInfo.FormatName);
        }

        var dateProperty = videoInfo.FormatTags.FirstOrDefault(k => string.Equals("creation_time", k.Key, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(dateProperty.Value) && DateTime.TryParse(dateProperty.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var tmp))
        {
            result.Add("CreatedDate", tmp.ToUniversalTime().ToString("O"));
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
                var videoInfo = FFProbe.GetMediaInfo(filepath);
                result = !(videoInfo.Duration == TimeSpan.Zero
                            || videoInfo.FormatName.StartsWith("image2")
                            || videoInfo.FormatName.EndsWith("_pipe")
                            || videoInfo.Streams.All(s => s.CodecType == "subtitle"));
            }
            catch
            {
                result = false;
            }
        }
        return result;
    }
}
