using MediaFinder.DataAccessLayer;
using MediaFinder.Logging;
using MediaFinder.Messages;
using MediaFinder.Models;

using Microsoft.Extensions.Logging;

namespace MediaFinder.DiscoveryServices;

public class DiscoveryRunnerService
{
    private readonly ILogger<DiscoveryRunnerService> _logger;
    private readonly MediaFinderDbContext _dbContext;

    public DiscoveryRunnerService(
        ILogger<DiscoveryRunnerService> logger,
        MediaFinderDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateRunContext(
        string workingDirectory,
        DiscoveryOptions configuration,
        IProgress<object> progressUpdate,
        CancellationToken cancellationToken = default)
    {
        progressUpdate.Report("Creating Discovery Context...");
        var runDetails = new DataAccessLayer.Models.DiscoveryExecution
        {
            ConfigurationId = configuration.Id,
            WorkingDirectory = workingDirectory
        };
        _dbContext.Runs.Add(runDetails);
        await _dbContext.SaveChangesAsync(cancellationToken);

        progressUpdate.Report("Preparing Working Directory...");
        var workingRunDirectory = Path.Combine(workingDirectory, runDetails.Id.ToString());
        Directory.CreateDirectory(workingRunDirectory);
        runDetails.WorkingDirectory = workingRunDirectory;
        await _dbContext.SaveChangesAsync(cancellationToken);

        progressUpdate.Report(WorkingDirectoryCreated.Create(workingRunDirectory));
        _logger.CreatedWorkingDirectory(workingRunDirectory);

        return runDetails.Id;
    }
}
