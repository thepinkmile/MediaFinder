using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MediaFinder_v2.Views.Executors
{
    /// <summary>
    /// Interaction logic for AddSearchSetting.xaml
    /// </summary>
    public partial class AddSearchSetting : UserControl
    {
        [GeneratedRegex($"(\\d+)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking)]
        private static partial Regex NumbersOnlyRegex();
        private static readonly Regex NumberOnlyRegex = NumbersOnlyRegex();

        public AddSearchSetting()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddSearchSettingViewModel viewModel)
            {
                viewModel.ClearFormCommand.Execute(this);
            }
        }

        private void TextBox_PreviewTextInput_NumericOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !NumberOnlyRegex.IsMatch(e.Text);
        }

        private void TextBox_Pasting_NumericOnly(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!NumberOnlyRegex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
