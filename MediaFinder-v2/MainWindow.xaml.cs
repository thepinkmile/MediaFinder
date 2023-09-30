using System.Windows.Input;

using MediaFinder_v2.Views.Executors;
using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(MainWindowsViewModel mainWindowViewModel,
        SearchSettingsViewModel searchSettingsViewModel,
        AddSearchSettingViewModel addSearchSettingViewModel,
        SearchExecutorViewModel searchExecutorViewModel)
    {
        DataContext = mainWindowViewModel;
        InitializeComponent();

        AddSearchSettingsView.DataContext = addSearchSettingViewModel;
        SearchSettingsView.DataContext = searchSettingsViewModel;
        Executor.DataContext = searchExecutorViewModel;

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
