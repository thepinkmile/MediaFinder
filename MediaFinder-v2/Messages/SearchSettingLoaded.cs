using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2.Messages;

public record SearchSettingLoaded(SearchSettingItemViewModel settings)
{
    public static SearchSettingLoaded Create(SearchSettingItemViewModel settings)
        => new(settings);
}
