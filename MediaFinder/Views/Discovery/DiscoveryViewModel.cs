using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MediaFinder.DataAccessLayer;
using MediaFinder.DiscoveryServices;
using MediaFinder.Helpers;
using MediaFinder.Logging;
using MediaFinder.Messages;
using MediaFinder.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediaFinder.Views.Discovery;

public partial class DiscoveryViewModel : ProgressableViewModel,
    IRecipient<SearchSettingUpdated>,
    IRecipient<WorkingDirectoryCreated>,
    IRecipient<FinishedMessage>,
    IRecipient<FileExtracted>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DiscoveryViewModel> _logger;
    private readonly Progress<object> _progressReporter;

    private Task? _discoveryTask;
    private CancellationTokenSource? _discoveryTaskCancellationSource;

    public DiscoveryViewModel(
        IServiceProvider serviceProvider,
        MediaFinderDbContext dbContext,
        IMessenger messenger,
        ILogger<DiscoveryViewModel> logger)
        : base(messenger, logger, dbContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        messenger.RegisterAll(this);

        BindingOperations.EnableCollectionSynchronization(Configurations, new());

        _progressReporter = new();
        _progressReporter.ProgressChanged += TmpProgressReporter_ProgressChanged;

        // TODO: Should this default to temp directory???
        WorkingDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TEMP");
    }

    private void TmpProgressReporter_ProgressChanged(object? sender, object e)
    {
        switch (e)
        {
            case WorkingDirectoryCreated wdcMessage: _messenger.Send(wdcMessage); break;
            case FileExtracted feMessage: _messenger.Send(feMessage); break;
            case string statusMessage: _messenger.Send(UpdateProgressMessage.Create(_progressToken!, statusMessage)); break;
        }
    }

    #region Settings Configurations

    [ObservableProperty]
    private UserControl? _drawContent;

    [ObservableProperty]
    private bool _editorVisible;

    [RelayCommand]
    private void OnAddSearchSetting()
    {
        DrawContent = _serviceProvider.GetRequiredService<AddSearchSettingView>();
        EditorVisible = true;
    }

    [RelayCommand]
    private async Task OnEditSearchSetting()
    {
        var view = _serviceProvider.GetRequiredService<EditSearchSettingView>();
        await view.InitializeDataContextAsync(SelectedConfig!.Id).ConfigureAwait(true);
        DrawContent = view;
        EditorVisible = true;
    }

    private bool CanRemoveSearchSetting()
        => SelectedConfig is not null;

    [RelayCommand(CanExecute = nameof(CanRemoveSearchSetting))]
    private async Task OnRemoveSearchSetting()
    {
        var config = SelectedConfig!;
        var entity = await _dbContext.SearchSettings
            .FirstOrDefaultAsync(x => x.Id == config.Id)
            .ConfigureAwait(true);
        if (entity is null)
        {
            return;
        }

        _dbContext.SearchSettings.Remove(entity);
        await _dbContext.SaveChangesAsync().ConfigureAwait(true);
        _messenger.Send(SearchSettingUpdated.Create(config));
        _messenger.Send(SnackBarMessage.Create($"Removed configuration: {config.Name}"));
    }

    public async void Receive(SearchSettingUpdated message)
    {
        try
        {
            if (LoadConfigurationsCommand.IsRunning)
                return;

            await LoadConfigurationsCommand.ExecuteAsync(null).ConfigureAwait(false);
        }
        catch
        {
            // do nothing
        }
    }

    #endregion

    #region Discovery Process

    [ObservableProperty]
    private ObservableCollection<DiscoveryOptions> _configurations = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    private string? _workingDirectory;

    [ObservableProperty]
    private ulong _workingDirectorySize;

    private string? _createdWorkingDirectory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveSearchSettingCommand))]
    private DiscoveryOptions? _selectedConfig;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(FinishSearchCommand))]
    private bool _searchComplete;

    partial void OnWorkingDirectoryChanged(string? value)
    {
        CleanupWorkingDirectory();
        SearchComplete = false;
    }

    partial void OnSelectedConfigChanged(DiscoveryOptions? value)
    {
        CleanupWorkingDirectory();
        SearchComplete = false;
    }

    [Obsolete("Will be replaced by Task.Run variant.")]
    public void Receive(WorkingDirectoryCreated message)
    {
        _createdWorkingDirectory = message.Directory;
    }

    [Obsolete("Will be replaced by Task.Run variant.")]
    public void Receive(FileExtracted message)
    {
        if (WorkingDirectory is null)
        {
            WorkingDirectorySize = 0UL;
            return;
        }

        var workingDir = new DirectoryInfo(WorkingDirectory!);
        if (!workingDir.Exists)
        {
            WorkingDirectorySize = 0UL;
            return;
        }

        WorkingDirectorySize = workingDir.GetDirectorySize();
    }

    [RelayCommand]
    public async Task LoadConfigurations()
    {
        ShowProgressIndicator("Loading...");

        Configurations.Clear();
        await foreach (var config in _dbContext.SearchSettings
            .Include(ss => ss.Directories)
            .AsAsyncEnumerable())
        {
            Configurations.Add(config.ToDiscoveryOptions());
        }

        HideProgressIndicator();
    }

    public bool CanPerformSearch()
        => !string.IsNullOrEmpty(WorkingDirectory)
            && SelectedConfig is not null
            && !SearchComplete
            && (_discoveryTask is null || _discoveryTask.IsCompleted)
            && (_discoveryTaskCancellationSource is null);

    [RelayCommand(CanExecute = nameof(CanPerformSearch))]
    public void OnPerformSearch()
    {
        if (SelectedConfig is null)
        {
            _messenger.Send(SnackBarMessage.Create("No configuration selected"));
            return;
        }

        if (WorkingDirectory is null)
        {
            _messenger.Send(SnackBarMessage.Create("No working directory selected"));
            return;
        }

        if (_discoveryTask is null || _discoveryTask.IsCompleted)
        {
            ShowProgressIndicator("Initializing search parameters", CancelSearchCommand);
            _discoveryTaskCancellationSource = new();
            _discoveryTask = Task.Run(async () =>
            {
                // TODO: update DiscoveryRunnerService to create new Db Tennant for run.
                await TruncateFileDetailStateAsync(_dbContext).ConfigureAwait(true);
                var contextId = await _serviceProvider.GetRequiredService<DiscoveryRunnerService>().CreateRunContext(WorkingDirectory!, SelectedConfig!, _progressReporter, _discoveryTaskCancellationSource.Token);

                // TODO: Add next steps


                // ON Cancelled: SearchCleanup();

                //_messenger.Send(SnackBarMessage.Create("Discovery process completed"));
                //SearchFinished();
            });
            CancelSearchCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanCancelSearch()
        => _discoveryTask is not null && !_discoveryTask.IsCompleted
            && _discoveryTaskCancellationSource is not null && !_discoveryTaskCancellationSource.IsCancellationRequested;

    [RelayCommand(CanExecute = nameof(CanCancelSearch))]
    private void OnCancelSearch()
    {
        CancelProgressIndicator("Cancelling...");
        _discoveryTaskCancellationSource!.Cancel();
        CancelSearchCommand.NotifyCanExecuteChanged();
    }

    private void SearchCleanup()
    {
        _discoveryTaskCancellationSource = null;
        _discoveryTask = null;

        CleanupWorkingDirectory();
        HideProgressIndicator();
        
        CancelSearchCommand.NotifyCanExecuteChanged();
        PerformSearchCommand.NotifyCanExecuteChanged();
    }

    private void SearchFinished()
    {
        _discoveryTaskCancellationSource = null;
        _discoveryTask = null;

        HideProgressIndicator();
        SearchComplete = true;
        
        _messenger.Send(DiscoveryCompletedMessage.Create());
        MoveToReviewCommand.Execute(null);
    }

    private bool CanMoveToReview()
        => SearchComplete
            && (_discoveryTask is null || _discoveryTask.IsCompleted);

    [RelayCommand(CanExecute = nameof(CanMoveToReview))]
    private void OnMoveToReview()
    {
        _messenger.Send(WizardNavigationMessage.Create(NavigationDirection.Next));
    }

    #endregion

    #region Finish Actions

    private void CleanupWorkingDirectory()
    {
        if (!string.IsNullOrEmpty(_createdWorkingDirectory)
            && Directory.Exists(_createdWorkingDirectory))
        {
            Directory.Delete(_createdWorkingDirectory, true);
            _logger.RemovedWorkingDirectory(_createdWorkingDirectory);
            _createdWorkingDirectory = null;
            WorkingDirectorySize = 0UL;
        }
    }

    public bool CanFinishSearch()
        => (_createdWorkingDirectory is not null || _dbContext.FileDetails.Any())
            && (_discoveryTask is null || _discoveryTask.IsCompleted)
            && _discoveryTaskCancellationSource is null;

    [RelayCommand(CanExecute = nameof(CanFinishSearch))]
    public void OnFinishSearch()
    {
        _messenger.Send(FinishedMessage.Create());
    }

    public async void Receive(FinishedMessage message)
    {
        try
        {
            await CleanupAsync().ConfigureAwait(true);
        }
        catch
        {
            // do nothing
        }
    }

    internal async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        _logger.SessionCleanup();

        await TruncateFileDetailStateAsync(_dbContext, cancellationToken).ConfigureAwait(true);

        CleanupWorkingDirectory();

        if (SearchComplete)
        {
            SearchComplete = false;
            SelectedConfig = null;
        }
    }

    #endregion
}
