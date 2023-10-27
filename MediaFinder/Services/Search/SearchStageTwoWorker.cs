using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Helpers;
using MediaFinder_v2.Logging;

using MetadataExtractor;

using Microsoft.Extensions.Logging;

using NReco.VideoInfo;

namespace MediaFinder_v2.Services.Search;

public partial class SearchStageTwoWorker : ReactiveBackgroundWorker<AnalyseRequest>
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
    private static readonly Enum[] MetadataTags = Enum.GetValues(typeof(TagLib.TagTypes)).Cast<Enum>().ToArray();

    internal const string FILENAME_DETAIL = "filename";
    internal const string FULLPATH_DETAIL = "fullPath";
    internal const string EXTENSION_DETAIL = "extension";
    internal const string PARENTPATH_DETAIL = "parentPath";
    internal const string PARENTNAME_DETAIL = "parentName";
    internal const string FILESIZE_DETAIL = "size";
    internal const string CREATEDDATE_DETAIL = "createdDate";
    internal const string MEDIATYPE_DETAIL = "mediaType";
    internal const string MD5_DETAIL = "md5";
    internal const string SHA256_DETAIL = "sha256";
    internal const string SHA512_DETAIL = "sha512";
    internal const string HEIGHT_DETAIL = "height";
    internal const string WIDTH_DETAIL = "width";
    internal const string FRAMERATE_DETAIL = "frameRate";
    internal const string EXPECTED_EXTENSION_DETAIL = "expectedExtension";
    internal const string WAS_EXTRACTED_DETAIL = "wasExtracted";
    internal const string RELATIVE_PATH_DETAIL = "relativePath";

    private readonly FFProbe _ffProbe;

    public SearchStageTwoWorker(
        ILogger<SearchStageTwoWorker> logger,
        IMessenger messenger,
        FFProbe ffProbe)
        : base(logger, messenger)
    {
        _ffProbe = ffProbe;
    }

    protected override void Execute(AnalyseRequest inputs, DoWorkEventArgs e)
    {
        var files = new List<FileDetails>();
        foreach(var filepath in inputs.Files)
        {
            if (CancellationPending)
            {
                break;
            }

            ReportProgress($"Analysing file: {filepath}");
            _logger.AnalysingFile(filepath);

            var details = new ConcurrentDictionary<string, string>();

            if (filepath.StartsWith(inputs.WorkingDirectory))
            {
                details.AddOrUpdate(WAS_EXTRACTED_DETAIL, "true");
                details.AddOrUpdate(RELATIVE_PATH_DETAIL, GetRelativePath(inputs.WorkingDirectory, filepath));
            }
            else
            {
                var originalSearchDir = inputs.OriginalPaths.FirstOrDefault(op => filepath.StartsWith(op));
                if (originalSearchDir is not null)
                {
                    details.AddOrUpdate(WAS_EXTRACTED_DETAIL, "false");
                    details.AddOrUpdate(RELATIVE_PATH_DETAIL, GetRelativePath(originalSearchDir, filepath));
                }
            }

            var fileInfo = new FileInfo(filepath);
            details.AddOrUpdate(FILENAME_DETAIL, fileInfo.Name);
            details.AddOrUpdate(FULLPATH_DETAIL, fileInfo.FullName);
            details.AddOrUpdate(FILESIZE_DETAIL, fileInfo.Length.ToString());
            details.AddOrUpdate(CREATEDDATE_DETAIL, fileInfo.CreationTimeUtc.ToString("O"));
            details.AddOrUpdate(PARENTPATH_DETAIL, fileInfo.DirectoryName!);
            details.AddOrUpdate(PARENTNAME_DETAIL, fileInfo.Directory!.Name);
            details.AddOrUpdate(EXTENSION_DETAIL, fileInfo.Extension.ToLowerInvariant());
            details.AddOrUpdate(MEDIATYPE_DETAIL, MultiMediaType.Unknown.ToStringFast());

            var cts = new CancellationTokenSource();

            // do initial basic media detection
            var metadataTask = GetMetadataInfo(filepath, details, inputs.PerformDeepAnalysis, cts.Token);
            while ((!metadataTask.IsCompleted || metadataTask.IsCompletedSuccessfully || metadataTask.IsFaulted || metadataTask.IsCanceled)
                && !cts.IsCancellationRequested)
            {
                Thread.Sleep(500);
                if (CancellationPending)
                {
                    cts.Cancel();
                }
            }
            metadataTask.Wait();

            // process detailed media info
            var processingTasks = new Task[] {
                GetVideoInfo(filepath, details, inputs.PerformDeepAnalysis, cts.Token),
                GetImageInfo(filepath, details, inputs.PerformDeepAnalysis, cts.Token),
                GetHashDetails(filepath, details, cts.Token)
            };
            while (!processingTasks.All(t => t.IsCompleted || t.IsCompletedSuccessfully || t.IsFaulted || t.IsCanceled)
                && !cts.IsCancellationRequested)
            {
                Thread.Sleep(500);
                if (CancellationPending)
                {
                    cts.Cancel();
                }
            }
            Task.WaitAll(processingTasks);

            if (CancellationPending)
            {
                break;
            }

            if (TryDescribeFile(details, out var fileDetail))
            {
                files.Add(fileDetail!);
            }
        }

        if (CancellationPending)
        {
            e.Cancel = true;
            return;
        }

        e.Result = AnalysisResponse.Create(files);
    }

    private string GetRelativePath(string sourceDirectory, string filepath)
    {
        return filepath[sourceDirectory.Length..];
    }

    private bool TryDescribeFile(ConcurrentDictionary<string, string> details, out FileDetails? fileDetails)
    {
        _logger.CompilingResults(details[FULLPATH_DETAIL]);

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
                FileType = MultiMediaTypeExtensions.TryParse(details[MEDIATYPE_DETAIL], out var value, true, true) ? value : MultiMediaType.Unknown,
                FileProperties = details.Select(x => new FileProperty() { Name = x.Key, Value = x.Value }).ToList(),
                MD5_Hash = details.GetValueOrDefault(MD5_DETAIL),
                SHA256_Hash = details.GetValueOrDefault(SHA256_DETAIL),
                SHA512_Hash = details.GetValueOrDefault(SHA512_DETAIL),
                Extracted = details.GetValueOrDefault(WAS_EXTRACTED_DETAIL, "false") == "true",
                RelativePath = details.TryGetValue(RELATIVE_PATH_DETAIL, out var path)
                    ? path
                    : details[PARENTNAME_DETAIL]
            };

            return true;
        }
        catch (Exception ex)
        {
            _logger.FailedToDescribeFile(ex, details[FULLPATH_DETAIL]);
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

    private async Task GetVideoInfo(string filepath, ConcurrentDictionary<string, string> details, bool performDeepAnalysis = false, CancellationToken cancellation = default)
    {
        if (!performDeepAnalysis)
        {
            _logger.PerformingVideoDetection(filepath);
            await Task.Yield();
            if (KnownVideoExtensions.Contains(details[EXTENSION_DETAIL]))
            {
                _logger.VideoDetected(filepath);
                await Task.Yield();
                details.AddOrUpdate(MEDIATYPE_DETAIL, MultiMediaType.Video.ToStringFast());
            }
            else
            {
                _logger.NoVideoDetected(filepath);
                await Task.Yield();
            }
            return;
        }

        try
        {
            _logger.PerformingVideoMetadataDetection(filepath);
            await Task.Yield();

            var videoInfo = _ffProbe.GetMediaInfo(filepath);
            if (videoInfo!.Duration == TimeSpan.Zero
                || videoInfo.FormatName.StartsWith("image2")
                || videoInfo.FormatName.EndsWith("_pipe")
                || videoInfo.Streams.All(s => s.CodecType == "subtitle"))
            {
                _logger.NoVideoDetected(filepath);
                await Task.Yield();
                return;
            }

            if (videoInfo.Streams.All(s => s.CodecType == "audio"))
            {
                _logger.AudioDetected(filepath);
                details.AddOrUpdate(MEDIATYPE_DETAIL, MultiMediaType.Audio.ToStringFast());

                var dateProperty = videoInfo.FormatTags.FirstOrDefault(k => string.Equals("date", k.Key, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(dateProperty.Value))
                {
                    if (DateTimeOffset.TryParseExact(dateProperty.Value, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal, out var createdYear))
                    {
                        details.AddOrUpdate("published_year", createdYear.ToUniversalTime().ToString("O"));
                    }
                    else
                    {
                        _logger.FailedToParseAudioDate(filepath, dateProperty.Value);
                    }
                }
            }
            else if (videoInfo.Streams.Any(s => s.CodecType == "video"))
            {
                _logger.VideoDetected(filepath);
                details.AddOrUpdate(MEDIATYPE_DETAIL, MultiMediaType.Video.ToStringFast());

                var dateProperty = videoInfo.FormatTags.FirstOrDefault(k => string.Equals("creation_time", k.Key, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(dateProperty.Value) && DateTimeOffset.TryParseExact(dateProperty.Value, IsoDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var createdDate))
                {
                    details.AddOrUpdate(CREATEDDATE_DETAIL, createdDate.ToUniversalTime().ToString("O"));
                }

                var firstVideoStream = videoInfo.Streams.FirstOrDefault(s => s.CodecType.Equals("video", StringComparison.InvariantCultureIgnoreCase));
                if (firstVideoStream is not null)
                {
                    details.AddOrUpdate(HEIGHT_DETAIL, firstVideoStream.Height.ToString());
                    details.AddOrUpdate(WIDTH_DETAIL, firstVideoStream.Width.ToString());
                    details.AddOrUpdate(FRAMERATE_DETAIL, firstVideoStream.FrameRate.ToString());
                }
            }
            else
            {
                _logger.NoVideoDetected(filepath);
                await Task.Yield();
                return;
            }
            details.AddOrUpdate("formatLongName", videoInfo!.FormatLongName);
            details.AddOrUpdate("formatName", videoInfo.FormatName);
            details.AddOrUpdate("duration", videoInfo.Duration.ToString("c", CultureInfo.InvariantCulture));

            var potentialExtensions = videoInfo.FormatName
                .Split(',')
                .Select(x => x.Trim().ToLowerInvariant())
                .ToArray();
            var detectedExtension = FindMatchingExtension(potentialExtensions, details[EXTENSION_DETAIL])
                ?? FindMatchingExtension(potentialExtensions, videoInfo.FormatLongName);
            if (detectedExtension is not null)
            {
                if (detectedExtension is "asf")
                {
                    detectedExtension = videoInfo.Streams
                        .Any(s => s.CodecType.Equals("video", StringComparison.InvariantCultureIgnoreCase))
                        ? "wmv"
                        : "wma";
                }

                details.AddOrUpdate(EXPECTED_EXTENSION_DETAIL, "." + detectedExtension);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.UserCanceledOperation("Meatdata Detection");
        }
        catch (Exception ex) when (ex is FFProbeException || ex.InnerException is FFProbeException)
        {
            _logger.FFProbeFailure(ex, filepath);
            await Task.Yield();
        }
        catch (Exception ex) when (ex.InnerException is not FFProbeException)
        {
            _logger.NoVideoDetected(filepath, ex);
            await Task.Yield();
        }
    }

    private string? FindMatchingExtension(string[] options, string extension)
        => options.FirstOrDefault(x => extension.Contains(x, StringComparison.InvariantCultureIgnoreCase));

    #endregion

    #region Image Processing

    private static readonly string[] KnownImageExtensions = new[] { ".bmp", ".jpg", ".jpeg", ".jfif", ".png", ".tif", ".tiff", ".gif", ".svg" };

    private const string REGEX_GROUP_PIXELS = "pixels";
    [GeneratedRegex($"(?<{REGEX_GROUP_PIXELS}>(\\d+))( {REGEX_GROUP_PIXELS})?", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex PixelsRegex();

    private async Task GetImageInfo(string filepath, ConcurrentDictionary<string, string> details, bool performDeepAnalysis = false, CancellationToken cancellationToken = default)
    {
        if (!performDeepAnalysis)
        {
            _logger.PerformingImageDetection(filepath);
            if (KnownImageExtensions.Contains(details[EXTENSION_DETAIL]))
            {
                _logger.ImageDetected(filepath);
                details.AddOrUpdate(MEDIATYPE_DETAIL, MultiMediaType.Image.ToStringFast());
            }
            else
            {
                _logger.NoImageDetected(filepath);
            }
            await Task.Yield();
            return;
        }

        try
        {
            _logger.PerformingImageMetadataDetection(filepath);
            await Task.Yield();

            using var fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var img = ImageMetadataReader.ReadMetadata(fs);
            if (img.Count == 0)
            {
                _logger.NoImageDetected(filepath);
                await Task.Yield();
                return;
            }
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();

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
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();

            details.AddOrUpdate(MEDIATYPE_DETAIL, MultiMediaType.Image.ToStringFast());

            var dateProperty = result.Keys.FirstOrDefault(k => k.Contains("Date/Time", StringComparison.InvariantCultureIgnoreCase));
            if (dateProperty is not null && DateTimeOffset.TryParseExact(result[dateProperty], IsoDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var tmp))
            {
                details.AddOrUpdate(CREATEDDATE_DETAIL, tmp.ToUniversalTime().ToString("O"));
            }

            ParseDimensionProperty(result, "Image Width", details, WIDTH_DETAIL);
            ParseDimensionProperty(result, "Image Height", details, HEIGHT_DETAIL);

            if (result.ContainsKey("File Type_Expected File Name Extension"))
            {
                details.AddOrUpdate(EXPECTED_EXTENSION_DETAIL, "." + result["File Type_Expected File Name Extension"]);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.UserCanceledOperation("Image Detection");
        }
        catch (Exception ex)
        {
            _logger.NoImageDetected(filepath, ex);
            await Task.Yield();
        }
    }

    private void ParseDimensionProperty(
        Dictionary<string, string> properties,
        string propertyIdentifier,
        ConcurrentDictionary<string, string> details,
        string outputPropertyIdentifier)
    {
        var widthProperty = properties.Keys.FirstOrDefault(k
            => k.Contains(propertyIdentifier, StringComparison.InvariantCultureIgnoreCase));
        if (widthProperty is not null)
        {
            if (long.TryParse(properties[widthProperty], out _))
            {
                details.AddOrUpdate(outputPropertyIdentifier, properties[widthProperty]);
                return;
            }

            var match = PixelsRegex().Match(properties[widthProperty]);
            var width = match.Groups[REGEX_GROUP_PIXELS].Value;
            if (!string.IsNullOrEmpty(width))
            {
                details.AddOrUpdate(outputPropertyIdentifier, width);
                return;
            }

            _logger.FailedToParseDimensionDetail(outputPropertyIdentifier, widthProperty, properties[widthProperty]);
        }
    }

    #endregion

    #region Metadata Processing

    private async Task GetMetadataInfo(string filepath, ConcurrentDictionary<string, string> details, bool performDeepAnalysis = false, CancellationToken cancellation = default)
    {
        await Task.Yield();

        if (!performDeepAnalysis)
        {
            _logger.SkippingBasicMetadataAnalysis(filepath);
            return;
        }

        try
        {
            _logger.PerformingMetadataDetection(filepath);
            cancellation.ThrowIfCancellationRequested();

            var fileMetadata = TagLib.File.Create(filepath, TagLib.ReadStyle.Average);
            cancellation.ThrowIfCancellationRequested();
            if (fileMetadata.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Photo))
            {
                SetImageMetadata(fileMetadata, details);
            }
            if (fileMetadata.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Audio))
            {
                SetImageMetadata(fileMetadata, details);
            }
            if (fileMetadata.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Video))
            {
                SetImageMetadata(fileMetadata, details);
            }
            if (fileMetadata.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.None))
            {
                _logger.UnknownMediaType(filepath);
            }
            if (fileMetadata.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Text))
            {
                _logger.TextMediaTypeDetected(filepath);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.UserCanceledOperation("Meatdata Detection");
        }
        catch (Exception ex)
        {
            _logger.FailedToReadMetadata(ex, filepath);
        }
    }

    private void SetImageMetadata(TagLib.File metadata, ConcurrentDictionary<string, string> details)
    {
        details.AddOrUpdate(MEDIATYPE_DETAIL, MultiMediaType.Image.ToStringFast());
        details.AddOrUpdate(WIDTH_DETAIL, metadata.Properties.PhotoWidth.ToString());
        details.AddOrUpdate(HEIGHT_DETAIL, metadata.Properties.PhotoHeight.ToString());

        if (metadata.Tag is TagLib.Image.CombinedImageTag imageTag)
        {
            if (imageTag.DateTime.HasValue)
            {
                details.AddOrUpdate(CREATEDDATE_DETAIL, imageTag.DateTime.Value.ToUniversalTime().ToString("O"));
            }
        }
    }

    private void SetVideoMetadata(TagLib.File metadata, ConcurrentDictionary<string, string> details)
    {
        details.AddOrUpdate(MEDIATYPE_DETAIL, MultiMediaType.Video.ToStringFast());
        details.AddOrUpdate(WIDTH_DETAIL, metadata.Properties.VideoWidth.ToString());
        details.AddOrUpdate(HEIGHT_DETAIL, metadata.Properties.VideoHeight.ToString());
        details.AddOrUpdate("duration", metadata.Properties.Duration.ToString("c", CultureInfo.InvariantCulture));
    }

    private void SetAudioMetadata(TagLib.File metadata, ConcurrentDictionary<string, string> details)
    {
        details.AddOrUpdate(MEDIATYPE_DETAIL, MultiMediaType.Audio.ToStringFast());
        details.AddOrUpdate("duration", metadata.Properties.Duration.ToString("c", CultureInfo.InvariantCulture));
        details.AddOrUpdate("audio_bitRate", metadata.Properties.AudioBitrate.ToString("c", CultureInfo.InvariantCulture));
        details.AddOrUpdate("audio_chanels", metadata.Properties.AudioChannels.ToString());
        details.AddOrUpdate("audio_sampleRate", metadata.Properties.AudioSampleRate.ToString());

        var earliestYear = MetadataTags
                    .Where(metadata.TagTypes.HasFlag)
                    .Cast<TagLib.TagTypes>()
                    .Where(tt => tt != TagLib.TagTypes.None)
                    .Select(tt => metadata.GetTag(tt))
                    .Where(tag => tag is not null && tag.Year != 0)
                    .Select(x => x.Year)
                    .Concat(new[] { Convert.ToUInt32(DateTimeOffset.UtcNow.Year) })
                    .Min()
                    .ToString();
        details.AddOrUpdate("published_year", earliestYear);
    }

    #endregion

    #region Hash Generation

    private async Task GetHashDetails(string filepath, ConcurrentDictionary<string, string> details, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.CreatingFileChecksum(filepath);
            await Task.Yield();

            using var fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var hashStream = new HashStream(fs, HashAlgorithmName.MD5, HashAlgorithmName.SHA256, HashAlgorithmName.SHA512);

            var read = 1024;
            var buffer = new byte[read];
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                read = hashStream.Read(buffer, 0, buffer.Length);
            } while (read != 0);

            await Task.Yield();
            details.AddOrUpdate(MD5_DETAIL, Convert.ToHexString(hashStream.Hash(HashAlgorithmName.MD5)));
            details.AddOrUpdate(SHA256_DETAIL, Convert.ToHexString(hashStream.Hash(HashAlgorithmName.SHA256)));
            details.AddOrUpdate(SHA512_DETAIL, Convert.ToHexString(hashStream.Hash(HashAlgorithmName.SHA512)));
        }
        catch (OperationCanceledException)
        {
            _logger.UserCanceledOperation("Meatdata Detection");
        }
        catch (Exception ex)
        {
            _logger.FailedToCreateChecksum(ex, filepath);
        }
        await Task.Yield();
    }

    #endregion
}