using MediaFinder_v2.Services.Search;

namespace MediaFinder_v2.Messages;

public record SearchSettingUpdated(SearchConfiguration SearchSetting)
{
    public static SearchSettingUpdated Create(DataAccessLayer.Models.SearchSettings settings)
        => Create(SearchConfiguration.Create(settings));

    public static SearchSettingUpdated Create(SearchConfiguration settings)
        => new(settings);
}
