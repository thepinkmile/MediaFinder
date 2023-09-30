using System.Windows.Controls;

namespace MediaFinder_v2.Views.SearchSettings;

/// <summary>
/// Interaction logic for SearchSettings.xaml
/// </summary>
public partial class SearchSettings : UserControl
{
    public SearchSettings()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is SearchSettingsViewModel viewModel)
        {
            await viewModel.LoadConfigurationsCommand.ExecuteAsync(null);
        }
    }
}
