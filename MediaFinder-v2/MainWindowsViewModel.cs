using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.Messages;
using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2;

public partial class MainWindowsViewModel : ObservableObject, IRecipient<SearchSettingLoaded>
{
    [ObservableProperty]
    private SearchSettingItemViewModel? _searchSettings;

    public MainWindowsViewModel(IMessenger messenger)
    {
        messenger.RegisterAll(this);
    }

    public void Receive(SearchSettingLoaded message)
    {
        SearchSettings = message.settings;
    }
}
