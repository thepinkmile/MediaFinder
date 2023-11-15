using System.Collections.ObjectModel;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignExtensions.Controls;

using MaterialDesignThemes.Wpf;

using MediaFinder.DataAccessLayer;
using MediaFinder.DataAccessLayer.Models;
using MediaFinder.Messages;

namespace MediaFinder.Views.Discovery;

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

    public AddSearchSettingViewModel(AppDbContext appDbContext, IMessenger messenger)
    {
        _dbContext = appDbContext;
        _messenger = messenger;
    }

    [RelayCommand]
#pragma warning disable CRR0034
#pragma warning disable CRR0035
    private async Task OnAddSearchDirectory()
#pragma warning restore CRR0035
#pragma warning restore CRR0034
    {
        var dialogResult = await OpenDirectoryDialog.ShowDialogAsync("DialogHost", new OpenDirectoryDialogArguments
        {
            CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        }).ConfigureAwait(true);
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
#pragma warning disable CRR0034
#pragma warning disable CRR0035
    private async Task OnSubmit()
#pragma warning restore CRR0035
#pragma warning restore CRR0034
    {
        var newSettings = new SearchSettings()
        {
            Name = SettingName!,
            Description = SettingDescription,
            Directories = SettingsDirectories.Select(p => new SearchDirectory { Path = p }).ToList(),
            Recursive = SettingRecursive,
            ExtractArchives = SettingExtractArchives,
            ExtractionDepth = SettingExtractArchives ? SettingExtractionDepth : null,
            PerformDeepAnalysis = SettingPerformDeepAnalysis,
            MinImageWidth = ImageSizesDefined ? MinImageWidth : null,
            MinImageHeight = ImageSizesDefined ? MinImageHeight : null,
            MinVideoWidth = VideoSizesDefined ? MinVideoWidth : null,
            MinVideoHeight = VideoSizesDefined ? MinVideoHeight : null
        };
        _dbContext.SearchSettings.Add(newSettings);
#pragma warning disable CRR0039
        await _dbContext.SaveChangesAsync().ConfigureAwait(true);
#pragma warning restore CRR0039

        ClearFormCommand.Execute(null);
        _messenger.Send(SnackBarMessage.Create("New search configuration saved"));
        _messenger.Send(SearchSettingUpdated.Create(newSettings));
        DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
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
        SettingExtractionDepth = 5;
        SettingPerformDeepAnalysis = false;
        ImageSizesDefined = false;
        MinImageWidth = null;
        MinImageHeight = null;
        VideoSizesDefined = false;
        MinVideoWidth = null;
        MinVideoHeight = null;
    }

    [RelayCommand]
    private void OnCancelAddSearchSetting()
    {
        ClearFormCommand.Execute(null);
        DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
    }
}
