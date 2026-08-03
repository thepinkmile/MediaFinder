using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MediaFinder.Views.Discovery;

/// <summary>
/// Interaction logic for AddSearchSettingView.xaml
/// </summary>
public partial class AddSearchSettingView : UserControl
{
    [GeneratedRegex("\\d+", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex NumbersOnlyRegex();

    public AddSearchSettingView()
    {
        InitializeComponent();
    }

    public AddSearchSettingView(AddSearchSettingViewModel viewModel)
    {
        DataContext = viewModel;
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
        e.Handled = !NumbersOnlyRegex().IsMatch(e.Text);
    }

    private void TextBox_Pasting_NumericOnly(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string));
            if (!NumbersOnlyRegex().IsMatch(text))
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
