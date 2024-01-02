namespace MediaFinder.DataAccessLayer.Models;

public class DiscoveryExecution
{
    public Guid Id { get; set; }

    public virtual SearchSettings Configuration { get; set; } = null!;

    public DateTimeOffset StartDateTime { get; set; } = DateTimeOffset.UtcNow;
}
