using System.Windows.Controls;

namespace MediaFinder.Views.Discovery
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

#pragma warning disable CRR0034
        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
#pragma warning restore CRR0034
        {
            try
            {
                if (DataContext is DiscoveryViewModel viewModel)
                {
                    await viewModel.LoadConfigurationsCommand.ExecuteAsync(null).ConfigureAwait(true);
                }
            }
            catch
            {
                // do nothing
            }
        }
    }
}
