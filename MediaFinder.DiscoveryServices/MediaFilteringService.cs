using MediaFinder.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using MediaFinder.Logging;
using MediaFinder.Models;

using Microsoft.Extensions.Logging;
using MediaFinder.Messages;

namespace MediaFinder.DiscoveryServices;

public class MediaFilteringService
{
    private readonly ILogger<MediaFilteringService> _logger;
    private readonly MediaFinderDbContext _dbContext;

    public MediaFilteringService(
        ILogger<MediaFilteringService> logger,
        MediaFinderDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Run(IProgress<object> progressUpdate, CancellationToken cancellationToken = default)
    {
        var inputs = FilterRequest.Create(null!, null, null, null, null);

        progressUpdate.Report($"Finalising Analysis Results...");
        _dbContext.FileDetails
            .Where(fd => fd.FileType == MultiMediaType.Unknown)
            .ExecuteUpdate(s => s.SetProperty(fd => fd.ShouldExport, false));
        _dbContext.ChangeTracker.Clear();

        progressUpdate.Report($"Suppressing duplicates from export list...");
        var hashGroups = _dbContext.FileDetails
            .Where(fd => fd.ShouldExport && fd.FileType != MultiMediaType.Unknown)
            .Select(fd => new { MD5 = fd.MD5_Hash, SHA256 = fd.SHA256_Hash, SHA512 = fd.SHA512_Hash })
            .Distinct();
        foreach (var group in hashGroups)
        {
            var files = _dbContext.FileDetails
                .Where(fd => fd.MD5_Hash == group.MD5
                        && fd.SHA256_Hash == group.SHA256
                        && fd.SHA512_Hash == group.SHA512
                        && fd.ShouldExport)
                .OrderBy(fd => fd.Extracted)
                .ThenBy(fd => fd.Created)
                .Skip(1)
                .ToList();
            if (files.Count != 0)
            {
                _logger.DuplicateChecksum(group.MD5, group.SHA256, group.SHA512, files.Count);
                foreach (var file in files)
                {
                    file.ShouldExport = false;
                }
            }
        }
        _dbContext.SaveChanges();

        progressUpdate.Report($"Suppressing small results from export list...");
        var minImageWidth = inputs.MinImageWidth;
        var minImageHeight = inputs.MinImageHeight;
        if (minImageWidth < minImageHeight)
        {
            _logger.ProtraitOrientationDetected("image config dimensions");
            (minImageHeight, minImageWidth) = (minImageWidth, minImageHeight);
        }
        var minVideoWidth = inputs.MinVideoWidth;
        var minVideoHeight = inputs.MinVideoHeight;
        if (minVideoWidth < minVideoHeight)
        {
            _logger.ProtraitOrientationDetected("video config dimensions");
            (minVideoHeight, minVideoWidth) = (minVideoWidth, minVideoHeight);
        }
        var mediaFiles = _dbContext.FileDetails
            .Include(fd => fd.FileProperties)
            .Where(fd => fd.ShouldExport && fd.FileType != MultiMediaType.Audio && fd.FileType != MultiMediaType.Unknown);
        foreach (var mediaFile in mediaFiles)
        {
            var heightValue = mediaFile.FileProperties
                .FirstOrDefault(fp => fp.Name.Equals("height", StringComparison.InvariantCultureIgnoreCase))
                ?.Value;
            if (string.IsNullOrEmpty(heightValue) || !long.TryParse(heightValue, out var height))
            {
                continue;
            }

            var widthValue = mediaFile.FileProperties
                .FirstOrDefault(fp => fp.Name.Equals("width", StringComparison.InvariantCultureIgnoreCase))
                ?.Value;
            if (string.IsNullOrEmpty(widthValue) || !long.TryParse(widthValue, out var width))
            {
                continue;
            }

            var minWidth = mediaFile.FileType == MultiMediaType.Image
                ? minImageWidth
                : minVideoWidth;
            var minHeight = mediaFile.FileType == MultiMediaType.Image
                ? minImageHeight
                : minVideoHeight;

            // detect orientation and ensure "Landscape" for comparisons
            if (width < height)
            {
                _logger.ProtraitOrientationDetected(mediaFile.FileName);
                (height, width) = (width, height);
            }

            if (width < minWidth && height < minHeight)
            {
                _logger.ExcludedBySize(mediaFile.FileName);
                mediaFile.ShouldExport = false;
            }
        }
        _dbContext.SaveChanges();

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        // TODO: store in database
        await Task.Delay(100, cancellationToken);
    }
}
