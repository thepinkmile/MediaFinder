using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Helpers;

using MetadataExtractor;

using Microsoft.Extensions.Logging;

using NReco.VideoInfo;

namespace MediaFinder_v2.Services.Search;

public partial class SearchStageTwoWorker : ReactiveBackgroundWorker
{
    private static readonly string[] IsoDateFormats = { 
        // Basic formats
        "yyyyMMddTHHmmsszzz",
        "yyyyMMddTHHmmsszz",
        "yyyyMMddTHHmmssZ",
        // Extended formats
        "yyyy-MM-ddTHH:mm:sszzz",
        "yyyy-MM-ddTHH:mm:sszz",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-dd HH:mm:sszzz",
        "yyyy-MM-dd HH:mm:sszz",
        "yyyy-MM-dd HH:mm:ssZ",
        // Precise formats
        "yyyyMMddTHHmmssFFFFFFFzzz",
        "yyyyMMddTHHmmssFFFFFFFzz",
        "yyyyMMddTHHmmssFFFFFFFZ",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFzz",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ",
        "yyyy-MM-dd HH:mm:ss.FFFFFFFzzz",
        "yyyy-MM-dd HH:mm:ss.FFFFFFFzz",
        "yyyy-MM-dd HH:mm:ss.FFFFFFFZ",
        // All of the above with reduced accuracy
        "yyyyMMddTHHmmzzz",
        "yyyyMMddTHHmmzz",
        "yyyyMMddTHHmmZ",
        "yyyy-MM-ddTHH:mmzzz",
        "yyyy-MM-ddTHH:mmzz",
        "yyyy-MM-ddTHH:mmZ",
        "yyyy-MM-dd HH:mmzzz",
        "yyyy-MM-dd HH:mmzz",
        "yyyy-MM-dd HH:mmZ",
        // Accuracy reduced to hours
        "yyyyMMddTHHzzz",
        "yyyyMMddTHHzz",
        "yyyyMMddTHHZ",
        "yyyy-MM-ddTHHzzz",
        "yyyy-MM-ddTHHzz",
        "yyyy-MM-ddTHHZ",
        "yyyy-MM-dd HHzzz",
        "yyyy-MM-dd HHzz",
        "yyyy-MM-dd HHZ",
        // Image metadata formats
        "yyyy:MM:dd HH:mm:sszzz",
        "yyyy:MM:dd HH:mm:sszz",
        "yyyy:MM:dd HH:mm:ssZ",
        "yyyy:MM:dd HH:mm:ss"
        };

    private const string FILENAME_DETAIL = "filename";
    private const string FULLPATH_DETAIL = "fullPath";
    private const string EXTENSION_DETAIL = "extension";
    private const string PARENTPATH_DETAIL = "parentPath";
    private const string FILESIZE_DETAIL = "size";
    private const string CREATEDDATE_DETAIL = "createdDate";
    private const string MEDIATYPE_DETAIL = "mediaType";
    private const string MD5_DETAIL = "md5";
    private const string SHA256_DETAIL = "sha256";
    private const string SHA512_DETAIL = "sha512";
    private const string HEIGHT_DETAIL = "height";
    private const string WIDTH_DETAIL = "width";
    private const string FRAMERATE_DETAIL = "frameRate";
    private const string EXPECTED_EXTENSION_DETAIL = "expectedExtension";

    private readonly FFProbe _ffProbe;

    public SearchStageTwoWorker(
        ILogger<SearchStageTwoWorker> logger,
        IMessenger messenger,
        FFProbe ffProbe)
        : base(logger, messenger)
    {
        _ffProbe = ffProbe;
    }

    protected override void Execute(object? sender, DoWorkEventArgs e)
    {
        if (e.Argument is not AnalyseRequest inputs) return;

        SetProgress("Initializing analizers...");

        var files = new ConcurrentBag<FileDetails>();
        Parallel.ForEach(
            inputs.Files,
            filepath =>
            {
                if (CancellationPending)
                {
                    return;
                }

                SetProgress($"Analysing file: {filepath}");

                var details = new Dictionary<string, string>();
                var fileInfo = new FileInfo(filepath);

                details.Add(FILENAME_DETAIL, fileInfo.Name);
                details.Add(FULLPATH_DETAIL, fileInfo.FullName);
                details.Add(FILESIZE_DETAIL, fileInfo.Length.ToString());
                details.Add(CREATEDDATE_DETAIL, fileInfo.CreationTimeUtc.ToString("O"));
                details.Add(PARENTPATH_DETAIL, fileInfo.DirectoryName!);
                details.Add(EXTENSION_DETAIL, fileInfo.Extension.ToLowerInvariant());
                details.Add(MEDIATYPE_DETAIL, Enum.GetName(MultiMediaType.Unknown)!);

                if (GetVideoInfo(filepath, details, inputs.PerformDeepAnalysis)
                    || GetImageInfo(filepath, details, inputs.PerformDeepAnalysis))
                {
                    if (CancellationPending)
                    {
                        return;
                    }

                    GetHashDetails(filepath, details);

                    if (TryDescribeFile(details, out var fileDetail))
                    {
                        files.Add(fileDetail!);
                    }
                }
            });

        if (CancellationPending)
        {
            e.Cancel = true;
            return;
        }

        var result = AnalysisResponse.Create();
        var i = 0;
        foreach(var file in files)
        {
            if (CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            SetProgress($"Finalising Analysis Results: {++i} of {files.Count}");
            result.Files.Add(file);
        }
        e.Result = result;
    }

    private bool TryDescribeFile(Dictionary<string, string> details, out FileDetails? fileDetails)
    {
        SetProgress($"Compilinig analysis result: {details[FULLPATH_DETAIL]}");

        try
        {
            fileDetails = new()
            {
                FileName = details[FILENAME_DETAIL],
                ParentPath = details[PARENTPATH_DETAIL],
                Created = DateTimeOffset.TryParseExact(details[CREATEDDATE_DETAIL], IsoDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var createdDate)
                        ? createdDate
                        : DateTimeOffset.UnixEpoch,
                FileSize = long.Parse(details[FILESIZE_DETAIL]),
                ShouldExport = true,
                FileType = details[MEDIATYPE_DETAIL].ToMultiMediaType(),
                FileProperties = details.Select(x => new FileProperty() { Name = x.Key, Value = x.Value }).ToList(),
                MD5_Hash = details.ContainsKey(MD5_DETAIL) ? details[MD5_DETAIL] : null,
                SHA256_Hash = details.ContainsKey(SHA256_DETAIL) ? details[SHA256_DETAIL] : null,
                SHA512_Hash = details.ContainsKey(SHA512_DETAIL) ? details[SHA512_DETAIL] : null
            };

            return true;
        }
        catch (Exception ex)
        {
            LogDebug($"Failed to describe file: {details[FULLPATH_DETAIL]}", ex);
            fileDetails = null;
            return false;
        }
    }

    #region Video Processing

    private static readonly string[] KnownVideoExtensions = new[] {
        "webm", "mkv", "flv", "vob", "ogv", "ogg", "rrc", "gifv", "mng", "mov",
        "avi", "qt", "wmv", "yuv", "rm", "asf", "amv", "mp4", "m4p", "m4v", "mpg",
        "mp2", "mpeg", "mpe", "mpv", "m4v", "svi", "3gp", "3g2", "mxf", "roq",
        "nsv", "flv", "f4v", "f4p", "f4a", "f4b", "mod" };

    private bool GetVideoInfo(string filepath, Dictionary<string, string> details, bool performDeepAnalysis = false)
    {
        if (!performDeepAnalysis)
        {
            LogDebug($"Performing file extension video detection: {filepath}");
            if (KnownVideoExtensions.Contains(details[EXTENSION_DETAIL]))
            {
                LogDebug($"Video detected: {filepath}");
                details[MEDIATYPE_DETAIL] = Enum.GetName(MultiMediaType.Video)!;
                return true;
            }
            else
            {
                LogDebug($"No video detected: {filepath}");
                return false;
            }
        }

        try
        {
            LogDebug($"Performing video metadata detection: {filepath}");
            var videoInfo = _ffProbe.GetMediaInfo(filepath);
            if (videoInfo!.Duration == TimeSpan.Zero
                || videoInfo.FormatName.StartsWith("image2")
                || videoInfo.FormatName.EndsWith("_pipe")
                || videoInfo.Streams.All(s => s.CodecType == "subtitle"))
            {
                LogDebug($"Failed to analyse file as video: {details[FILENAME_DETAIL]}");
                return false;
            }

            details[MEDIATYPE_DETAIL] = Enum.GetName(MultiMediaType.Video)!;
            details.Add("formatLongName", videoInfo!.FormatLongName);
            details.Add("formatName", videoInfo.FormatName);
            details.Add("duration", videoInfo.Duration.ToString());

            var dateProperty = videoInfo.FormatTags.FirstOrDefault(k => string.Equals("creation_time", k.Key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(dateProperty.Value) && DateTimeOffset.TryParseExact(dateProperty.Value, IsoDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var createdDate))
            {
                details[CREATEDDATE_DETAIL] = createdDate.ToUniversalTime().ToString("O");
            }

            var firstVideoStream = videoInfo.Streams.FirstOrDefault(s => s.CodecType.Equals("video", StringComparison.InvariantCultureIgnoreCase));
            if (firstVideoStream is not null)
            {
                details.Add(HEIGHT_DETAIL, firstVideoStream.Height.ToString());
                details.Add(WIDTH_DETAIL, firstVideoStream.Width.ToString());
                details.Add(FRAMERATE_DETAIL, firstVideoStream.FrameRate.ToString());
            }

            // TODO: fix this
            var potentialExtensions = videoInfo.FormatName
                .Split(',')
                .Select(x => x.Trim().ToLowerInvariant())
                .ToArray();
            var detectedExtension = FindMatchingExtension(potentialExtensions, details[EXTENSION_DETAIL])
                ?? FindMatchingExtension(potentialExtensions, videoInfo.FormatLongName);
            var tmp = details[EXTENSION_DETAIL];
            if (detectedExtension is not null)
            {
                if (detectedExtension is "asf")
                {
                    detectedExtension = videoInfo.Streams
                        .Any(s => s.CodecType.Equals("video", StringComparison.InvariantCultureIgnoreCase))
                        ? "wmv"
                        : "wma";
                }

                details.Add(EXPECTED_EXTENSION_DETAIL, "." + detectedExtension);
            }

            return true;
        }
        catch (Exception ex) when (ex is FFProbeException || ex.InnerException is FFProbeException)
        {
            LogDebug($"No video metadata detected: {details[FILENAME_DETAIL]}", ex);
            return false;
        }
        catch (Exception ex) when (ex.InnerException is not FFProbeException)
        {
            LogDebug($"Failed to analyse file as video: {details[FILENAME_DETAIL]}", ex);
            return false;
        }
    }

    private string? FindMatchingExtension(string[] options, string extension)
        => options.FirstOrDefault(x => extension.Contains(x, StringComparison.InvariantCultureIgnoreCase));

    #endregion

    #region Image Processing

    private static readonly string[] KnownImageExtensions = new[] { ".bmp", ".jpg", ".jpeg", ".jfif", ".png", ".tif", ".tiff", ".gif", ".svg" };

    private const string REGEX_GROUP_PIXELS = "pixels";
    [GeneratedRegex($"(?<{REGEX_GROUP_PIXELS}>(\\d+)) {REGEX_GROUP_PIXELS}", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex PixelsRegex();
    private static readonly Regex PixelsPropertyParser = PixelsRegex();

    private bool GetImageInfo(string filepath, Dictionary<string, string> details, bool performDeepAnalysis = false)
    {
        if (!performDeepAnalysis)
        {
            LogDebug($"Performing file extension image detection: {filepath}");
            if (KnownImageExtensions.Contains(details[EXTENSION_DETAIL]))
            {
                LogDebug($"Image detected: {filepath}");
                details[MEDIATYPE_DETAIL] = Enum.GetName(MultiMediaType.Image)!;
                return true;
            }
            else
            {
                LogDebug($"No image detected: {filepath}");
                return false;
            }
        }

        try
        {
            LogDebug($"Performing image metadata detection: {filepath}");
            using var fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var img = ImageMetadataReader.ReadMetadata(fs);
            if (img.Count == 0)
            {
                LogDebug($"Failed to analyse file as image: {details[FILENAME_DETAIL]}");
                return false;
            }

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

            details[MEDIATYPE_DETAIL] = Enum.GetName(MultiMediaType.Image)!;

            var dateProperty = result.Keys.FirstOrDefault(k => k.Contains("Date/Time", StringComparison.InvariantCultureIgnoreCase));
            if (dateProperty is not null && DateTimeOffset.TryParseExact(result[dateProperty], IsoDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var tmp))
            {
                details[CREATEDDATE_DETAIL] = tmp.ToUniversalTime().ToString("O");
            }

            var widthProperty = result.Keys.FirstOrDefault(k => k.Contains("Image Width", StringComparison.InvariantCultureIgnoreCase));
            if (widthProperty is not null)
            {
                var match = PixelsPropertyParser.Match(result[widthProperty]);
                var width = match.Groups[REGEX_GROUP_PIXELS].Value;
                details.Add(WIDTH_DETAIL, width);
            }

            var heightProperty = result.Keys.FirstOrDefault(k => k.Contains("Image Height", StringComparison.InvariantCultureIgnoreCase));
            if (heightProperty is not null)
            {
                var match = PixelsPropertyParser.Match(result[heightProperty]);
                var height = match.Groups[REGEX_GROUP_PIXELS].Value;
                details.Add(HEIGHT_DETAIL, height);
            }

            if (result.ContainsKey("File Type_Expected File Name Extension"))
            {
                details.Add(EXPECTED_EXTENSION_DETAIL, "." + result["File Type_Expected File Name Extension"]);
            }

            return true;
        }
        catch(Exception ex)
        {
            LogDebug($"Failed to analyse file as image: {details[FILENAME_DETAIL]}", ex);
            return false;
        }
    }

    #endregion

    #region Hash Generation

    private void GetHashDetails(string filepath, Dictionary<string, string> details)
    {
        try
        {
            SetProgress($"Creating file checksum: {filepath}");
            using var fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var hashStream = new HashStream(fs, HashAlgorithmName.MD5, HashAlgorithmName.SHA256, HashAlgorithmName.SHA512);

            var read = 1024;
            var buffer = new byte[read];
            do
            {
                if (CancellationPending)
                {
                    throw new OperationCanceledException("User cancelled analysis of file.");
                }

                read = hashStream.Read(buffer, 0, buffer.Length);
            } while (read != 0);

            details.Add(MD5_DETAIL, Convert.ToHexString(hashStream.Hash(HashAlgorithmName.MD5)));
            details.Add(SHA256_DETAIL, Convert.ToHexString(hashStream.Hash(HashAlgorithmName.SHA256)));
            details.Add(SHA512_DETAIL, Convert.ToHexString(hashStream.Hash(HashAlgorithmName.SHA512)));
        }
        catch (Exception ex)
        {
            LogDebug($"Failed to generate checksum for file: {details[FILENAME_DETAIL]}", ex);
        }
    }

    #endregion
}