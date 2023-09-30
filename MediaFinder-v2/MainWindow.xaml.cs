using System.Windows.Input;

using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(MainWindowsViewModel mainWindowViewModel ,SearchSettingsViewModel searchSettingsViewModel, AddSearchSettingViewModel addSearchSettingViewModel)
    {
        DataContext = mainWindowViewModel;
        InitializeComponent();

        AddSearchSettingsView.DataContext = addSearchSettingViewModel;
        SearchSettingsView.DataContext = searchSettingsViewModel;

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
