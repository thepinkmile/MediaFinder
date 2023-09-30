using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2.Messages;

public record SearchSettingLoaded(SearchSettingItemViewModel Settings)
{
    public static SearchSettingLoaded Create(SearchSettingItemViewModel settings)
        => new(settings);
}
