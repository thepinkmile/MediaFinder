using System.Collections.ObjectModel;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.Messages;
using MediaFinder_v2.Services.Search;

using Microsoft.EntityFrameworkCore;

namespace MediaFinder_v2.Views.SearchSettings;

public partial class SearchSettingsViewModel : ObservableObject, IRecipient<SearchSettingUpdated>
{
    private readonly AppDbContext _dbContext;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private ObservableCollection<SearchConfiguration> _configurations = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSearchSettingCommand))]
    private SearchConfiguration? _selectedConfig;

    public SearchSettingsViewModel(AppDbContext appDbContext, IMessenger messenger)
    {
        _dbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
        _messenger = messenger;
        messenger.RegisterAll(this);
        BindingOperations.EnableCollectionSynchronization(Configurations, new());
    }

    [RelayCommand]
    public async Task LoadConfigurations()
    {
        _messenger.Send(ShowProgressBar.Create("Loading..."));
        Configurations.Clear();

#if DEBUG
        await Task.Delay(2_000);
#endif

        await foreach (var config in _dbContext.SearchSettings.AsAsyncEnumerable())
        {
            Configurations.Add(SearchConfiguration.Create(config));
        }
        _messenger.Send(HideProgressBar.Create());
    }

    [RelayCommand]
    private void OnAddSearchSetting()
    {
        _messenger.Send(ChangeTab.ToAddSettingTab());
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSearchSetting))]
    private async Task OnRemoveSearchSetting()
    {
        var config = SelectedConfig!;
        var entity = await _dbContext.SearchSettings.FirstOrDefaultAsync(x => x.Id == config.Id);
        if (entity is null)
        {
            return;
        }

        _dbContext.SearchSettings.Remove(entity);
        await _dbContext.SaveChangesAsync();
        _messenger.Send(SearchSettingUpdated.Create(config));
        _messenger.Send(SnackBarMessage.Create($"Removed configuration: {config.Name}"));
    }

    private bool CanRemoveSearchSetting()
        => SelectedConfig is not null;

    public async void Receive(SearchSettingUpdated message)
    {
        if (LoadConfigurationsCommand.IsRunning)
            return;

        await LoadConfigurationsCommand.ExecuteAsync(null);
    }
}
