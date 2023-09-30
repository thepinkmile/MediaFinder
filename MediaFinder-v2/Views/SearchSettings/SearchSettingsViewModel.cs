using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Messages;

using Microsoft.EntityFrameworkCore;

namespace MediaFinder_v2.Views.SearchSettings;

public partial class SearchSettingsViewModel : ObservableObject, IRecipient<SearchSettingUpdated>
{
    private readonly AppDbContext _dbContext;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private ObservableCollection<SearchSettingItemViewModel> _configurations = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSearchSettingCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadSearchSettingCommand))]
    private SearchSettingItemViewModel? _selectedConfig;

    public SearchSettingsViewModel(AppDbContext appDbContext, IMessenger messenger)
    {
        _dbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
        _messenger = messenger;
        _messenger.RegisterAll(this);
        BindingOperations.EnableCollectionSynchronization(Configurations, new());
    }

    [RelayCommand]
    public async Task LoadConfigurations()
    {
        Configurations.Clear();

#if DEBUG
        await Task.Delay(2000);
#endif

        await foreach (var config in _dbContext.SearchSettings.AsAsyncEnumerable())
        {
            Configurations.Add(new SearchSettingItemViewModel(config));
        }
    }

    [RelayCommand]
    private async Task OnAddSearchSetting()
    {
        await Task.Delay(1000);
        // TODO: open AddSearchSetting Dialog
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSearchSetting))]
    private async Task OnRemoveSearchSetting()
    {
        var entity = await _dbContext.SearchSettings.FirstOrDefaultAsync(x => x.Id == SelectedConfig!.Id);
        if (entity is null)
        {
            return;
        }

        _dbContext.SearchSettings.Remove(entity);
        await _dbContext.SaveChangesAsync();
        _messenger.Send(SearchSettingUpdated.Create(SelectedConfig!));
    }

    private bool CanRemoveSearchSetting()
        => SelectedConfig is not null;

    [RelayCommand(CanExecute = nameof(CanRemoveSearchSetting))]
    private void OnLoadSearchSetting()
    {
        _messenger.Send(SearchSettingLoaded.Create(SelectedConfig!));
        MessageBox.Show($"{SelectedConfig!.Name} configuration has been set.", "Configuration Loaded", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
    }

    public async void Receive(SearchSettingUpdated message)
    {
        await LoadConfigurationsCommand.ExecuteAsync(null);
    }
}
