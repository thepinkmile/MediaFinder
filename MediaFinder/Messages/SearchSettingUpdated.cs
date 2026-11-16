using MediaFinder.DataAccessLayer.Models;
using MediaFinder.Helpers;
using MediaFinder.Models;

namespace MediaFinder.Messages;

public record SearchSettingUpdated(DiscoveryOptions SearchSetting)
{
    public static SearchSettingUpdated Create(SearchSettings settings)
        => Create(settings.ToDiscoveryOptions());

    public static SearchSettingUpdated Create(DiscoveryOptions options)
        => new(options);
}
