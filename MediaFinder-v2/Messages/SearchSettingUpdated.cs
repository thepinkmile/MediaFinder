using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2.Messages;

public record SearchSettingUpdated(SearchSettingItemViewModel searchSetting)
{
    public static SearchSettingUpdated Create(DataAccessLayer.Models.SearchSettings settings)
        => Create(new SearchSettingItemViewModel(settings));

    public static SearchSettingUpdated Create(SearchSettingItemViewModel settings)
        => new(settings);
}
