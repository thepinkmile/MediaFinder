using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignExtensions.Controls;

using MaterialDesignThemes.Wpf;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Messages;

namespace MediaFinder_v2.Views.SearchSettings;

public partial class AddSearchSettingViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string? _settingName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string? _settingDescription;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private bool _settingRecursive;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private bool _settingExtractArchives;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private bool _settingPerformDeepAnalysis;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private ObservableCollection<string> _settingsDirectories = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSearchDirectoryCommand))]
    private string? _selectedDirectory;

    public AddSearchSettingViewModel(AppDbContext appDbContext, IMessenger messenger)
    {
        _dbContext = appDbContext;
        _messenger = messenger;
    }

    [RelayCommand]
    private async Task OnAddSearchDirectory()
    {
        var dialogResult = await OpenDirectoryDialog.ShowDialogAsync("DialogHost", new OpenDirectoryDialogArguments
        {
            CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        });
        if (dialogResult.Confirmed && !string.IsNullOrEmpty(dialogResult.Directory))
        {
            SettingsDirectories.Add(dialogResult.Directory);
        }
        SubmitCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSearchDirectory))]
    private void OnRemoveSearchDirectory()
    {
        var item = SelectedDirectory!;
        SettingsDirectories.Remove(item);
        SubmitCommand.NotifyCanExecuteChanged();
    }

    private bool CanRemoveSearchDirectory()
        => !string.IsNullOrEmpty(SelectedDirectory);

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task OnSubmit()
    {
        var newSettings = new DataAccessLayer.Models.SearchSettings()
        {
            Name = SettingName!,
            Description = SettingDescription,
            Directories = SettingsDirectories.Select(p => new SearchDirectory { Path = p }).ToList(),
            Recursive = SettingRecursive,
            ExtractArchives = SettingExtractArchives,
            PerformDeepAnalysis = SettingPerformDeepAnalysis
        };
        _dbContext.SearchSettings.Add(newSettings);
        await _dbContext.SaveChangesAsync();

        ClearFormCommand.Execute(null);
        _messenger.Send(SnackBarMessage.Create("New search configuration saved"));

        _messenger.Send(ChangeTab.ToViewSettingsTab());
    }

    private bool CanSubmit()
        => !string.IsNullOrEmpty(SettingName) && SettingsDirectories.Any();

    [RelayCommand]
    private void OnClearForm()
    {
        SettingName = string.Empty;
        SettingDescription = null;
        SettingsDirectories.Clear();
        SettingRecursive = false;
        SettingExtractArchives = false;
        SettingPerformDeepAnalysis = false;
    }
}
