using System.Windows.Controls;

namespace MediaFinder_v2.Views.Executors
{
    /// <summary>
    /// Interaction logic for AddSearchSetting.xaml
    /// </summary>
    public partial class AddSearchSetting : UserControl
    {
        public AddSearchSetting()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AddSearchSettingViewModel viewModel)
            {
                viewModel.ClearFormCommand.Execute(this);
            }
        }
    }
}
