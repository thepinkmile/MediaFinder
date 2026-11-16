using System.ComponentModel;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder.DataAccessLayer;
using MediaFinder.Models;
using MediaFinder.Helpers;
using MediaFinder.Logging;
using MediaFinder.Messages;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaFinder.Services.Search;

public class SearchStageThreeWorker : ReactiveBackgroundWorker<FilterRequest>
{
    private readonly MediaFinderDbContext _dbContext;

    public SearchStageThreeWorker(
        ILogger<SearchStageTwoWorker> logger,
        IMessenger messenger,
        MediaFinderDbContext dbContext)
        : base(logger, messenger)
    {
        _dbContext = dbContext;
    }

    protected override void Execute(FilterRequest inputs, DoWorkEventArgs e)
    {
        SetProgress($"Finalising Analysis Results...");
        _dbContext.FileDetails
            .Where(fd => fd.FileType == MultiMediaType.Unknown)
            .ExecuteUpdate(s => s.SetProperty(fd => fd.ShouldExport, false));
        _dbContext.ChangeTracker.Clear();

        SetProgress($"Suppressing duplicates from export list...");
        var hashGroups = _dbContext.FileDetails
            .Where(fd => fd.ShouldExport && fd.FileType != MultiMediaType.Unknown)
            .Select(fd => new { MD5 = fd.MD5_Hash, SHA256 = fd.SHA256_Hash, SHA512 = fd.SHA512_Hash })
            .Distinct();
        foreach(var group in hashGroups)
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

        SetProgress($"Suppressing small results from export list...");
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
        foreach(var mediaFile in mediaFiles)
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

        if (CancellationPending)
        {
            e.Cancel = true;
            return;
        }

        e.Result = true;
    }
}
