using System.ComponentModel;
using System.Windows.Input;

using MediaFinder_v2.Views.Executors;
using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly SearchExecutorViewModel _searchExecutorViewModel;

    public MainWindow(MainWindowsViewModel mainWindowViewModel,
        SearchSettingsViewModel searchSettingsViewModel,
        AddSearchSettingViewModel addSearchSettingViewModel,
        SearchExecutorViewModel searchExecutorViewModel)
    {
        DataContext = mainWindowViewModel;
        InitializeComponent();

        AddSearchSettingsView.DataContext = addSearchSettingViewModel;
        SearchSettingsView.DataContext = searchSettingsViewModel;
        Executor.DataContext = _searchExecutorViewModel = searchExecutorViewModel;

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
        await _searchExecutorViewModel.FinishCommand.ExecuteAsync(null);
        base.OnClosing(e);
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
