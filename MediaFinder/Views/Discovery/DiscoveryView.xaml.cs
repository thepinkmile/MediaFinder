using System.Windows.Controls;

namespace MediaFinder_v2.Views.Discovery
{
    /// <summary>
    /// Interaction logic for DiscoveryView.xaml
    /// </summary>
    public partial class DiscoveryView : UserControl
    {
        public DiscoveryView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is DiscoveryViewModel viewModel)
            {
                await viewModel.LoadConfigurationsCommand.ExecuteAsync(null);
            }
        }
    }
}
