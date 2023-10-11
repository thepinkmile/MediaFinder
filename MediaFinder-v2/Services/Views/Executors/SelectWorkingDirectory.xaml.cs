using System.Windows.Controls;

namespace MediaFinder_v2.Views.Executors
{
    /// <summary>
    /// Interaction logic for SelectWorkingDirectory.xaml
    /// </summary>
    public partial class SelectWorkingDirectory : UserControl
    {
        public SelectWorkingDirectory()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SearchExecutorViewModel viewModel)
            {
                await viewModel.LoadConfigurationsCommand.ExecuteAsync(null);
            }
        }
    }
}
