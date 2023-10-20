using System.ComponentModel;

using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Services.Search;

public class SearchStageThreeWorker : ReactiveBackgroundWorker<FilterRequest>
{
    private readonly AppDbContext _dbContext;

    public SearchStageThreeWorker(ILogger<SearchStageTwoWorker> logger,
        IMessenger messenger, AppDbContext dbContext)
        : base(logger, messenger)
    {
        _dbContext = dbContext;
    }

    protected override void Execute(FilterRequest inputs, DoWorkEventArgs e)
    {
        SetProgress($"Finalising Analysis Results...");
        
        var initialCount = _dbContext.FileDetails.Count(fd => fd.ShouldExport);
        _dbContext.FileDetails
            .Where(fd => fd.FileType != MultiMediaType.Image && fd.FileType != MultiMediaType.Video)
            .ExecuteUpdate(s => s.SetProperty(fd => fd.ShouldExport, false));
        _dbContext.ChangeTracker.Clear();
        var filteredCountalCount = _dbContext.FileDetails.Count(fd => fd.ShouldExport);

        SetProgress($"Suppressing duplicates from export list...");
        var hashGroups = _dbContext.FileDetails
            .Where(fd => fd.ShouldExport)
            .Select(fd => new { MD5 = fd.MD5_Hash, SHA256 = fd.SHA256_Hash, SHA512 = fd.SHA512_Hash })
            .Distinct();
        foreach(var group in hashGroups)
        {
            var files = _dbContext.FileDetails
                .Where(fd => fd.MD5_Hash == group.MD5 && fd.SHA256_Hash == group.SHA256 && fd.SHA512_Hash == group.SHA512 && fd.ShouldExport)
                .OrderBy(fd => fd.Extracted)
                .ThenBy(fd => fd.Created)
                .Skip(1)
                .ToList();
            if (files.Any())
            {
                foreach (var file in files)
                {
                    file.ShouldExport = false;
                }
            }
        }
        _dbContext.SaveChanges();
        var deduplicatedCount = _dbContext.FileDetails.Count(fd => fd.ShouldExport);

        SetProgress($"Suppressing small results from export list...");
        var mediaFiles = _dbContext.FileDetails.Include(fd => fd.FileProperties).Where(fd => fd.ShouldExport);
        foreach(var mediaFile in mediaFiles)
        {
            var heightValue = mediaFile.FileProperties.FirstOrDefault(fp => fp.Name == "height")?.Value;
            if (string.IsNullOrEmpty(heightValue) || !long.TryParse(heightValue, out var height))
            {
                continue;
            }

            var widthValue = mediaFile.FileProperties.FirstOrDefault(fp => fp.Name == "width")?.Value;
            if (string.IsNullOrEmpty(widthValue) || !long.TryParse(widthValue, out var width))
            {
                continue;
            }

            var minWidth = mediaFile.FileType == MultiMediaType.Image
                ? inputs.MinImageWidth
                : inputs.MinVideoWidth;
            var minHeight = mediaFile.FileType == MultiMediaType.Image
                ? inputs.MinImageHeight
                : inputs.MinVideoHeight;

            // detect orientation and ensure "Landscape" for comparisons
            if (width < height)
            {
                (height, width) = (width, height);
            }
            if (minWidth < minHeight)
            {
                (minHeight, minWidth) = (minWidth, minHeight);
            }

            if (width < minWidth && height < minHeight)
            {
                mediaFile.ShouldExport = false;
            }
        }
        _dbContext.SaveChanges();
        var finalCount = _dbContext.FileDetails.Count(fd => fd.ShouldExport);

        if (CancellationPending)
        {
            e.Cancel = true;
            return;
        }

        e.Result = true;
    }
}
