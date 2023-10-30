using System.Collections.ObjectModel;

using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignExtensions.Controls;

using MaterialDesignThemes.Wpf;

using MediaFinder_v2.DataAccessLayer.Models;

using MediaFinder_v2.DataAccessLayer;

using MediaFinder_v2.Messages;

namespace MediaFinder_v2.Views.Discovery;

public partial class EditSearchSettingViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext;
    private readonly IMessenger _messenger;

    private SearchSettings? _entity;

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
    private int _settingExtractionDepth;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private bool _settingPerformDeepAnalysis;

    [ObservableProperty]
    private bool _imageSizesDefined;

    [ObservableProperty]
    private long? _minImageWidth;

    [ObservableProperty]
    private long? _minImageHeight;

    [ObservableProperty]
    private bool _videoSizesDefined;

    [ObservableProperty]
    private long? _minVideoWidth;

    [ObservableProperty]
    private long? _minVideoHeight;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private ObservableCollection<string> _settingsDirectories = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSearchDirectoryCommand))]
    private string? _selectedDirectory;

    public EditSearchSettingViewModel(AppDbContext appDbContext, IMessenger messenger)
    {
        _dbContext = appDbContext;
        _messenger = messenger;
    }

    public async Task Initialize(int settingId)
    {
        _entity = await _dbContext.SearchSettings.FindAsync(settingId)
            ?? throw new InvalidOperationException("Cannot edit configuration as it does not exist.");

        ResetFormCommand.Execute(null);
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
        _entity!.Name = SettingName!;
        _entity.Description = SettingDescription;
        _entity.Directories = SettingsDirectories.Select(p => new SearchDirectory { Path = p }).ToList();
        _entity.Recursive = SettingRecursive;
        _entity.ExtractArchives = SettingExtractArchives;
        _entity.ExtractionDepth = SettingExtractArchives ? SettingExtractionDepth : null;
        _entity.PerformDeepAnalysis = SettingPerformDeepAnalysis;
        _entity.MinImageWidth = ImageSizesDefined ? MinImageWidth : null;
        _entity.MinImageHeight = ImageSizesDefined ? MinImageHeight : null;
        _entity.MinVideoWidth = VideoSizesDefined ? MinVideoWidth : null;
        _entity.MinVideoHeight = VideoSizesDefined ? MinVideoHeight : null;

        _dbContext.SearchSettings.Update(_entity);
        await _dbContext.SaveChangesAsync();

        _messenger.Send(SnackBarMessage.Create("Search configuration updated"));
        _messenger.Send(SearchSettingUpdated.Create(_entity));
        DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
        _entity = null;
    }

    private bool CanSubmit()
        => !string.IsNullOrEmpty(SettingName) && SettingsDirectories.Any();

    [RelayCommand]
    private void OnResetForm()
    {
        SettingName = _entity!.Name;
        SettingDescription = _entity.Description;
        SettingRecursive = _entity.Recursive;
        SettingExtractArchives = _entity.ExtractArchives;
        SettingExtractionDepth = _entity.ExtractionDepth ?? 5;
        SettingPerformDeepAnalysis = _entity.PerformDeepAnalysis;
        SettingsDirectories.Clear();
        foreach(var directory in _entity.Directories)
        {
            SettingsDirectories.Add(directory.Path);
        }
        ImageSizesDefined = _entity.MinImageHeight is not null || _entity.MinImageWidth is not null;
        MinImageWidth = _entity.MinImageWidth;
        MinImageHeight = _entity.MinImageHeight;
        VideoSizesDefined = _entity.MinVideoHeight is not null || _entity.MinVideoWidth is not null;
        MinVideoWidth = _entity.MinVideoWidth;
        MinVideoHeight = _entity.MinVideoHeight;
    }

    [RelayCommand]
    private void OnCancelAddSearchSetting()
    {
        DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
        _entity = null;
    }
}
