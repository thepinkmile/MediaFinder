using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MediaFinder.Views.Discovery;

/// <summary>
/// Interaction logic for EditSearchSettingView.xaml
/// </summary>
public partial class EditSearchSettingView : UserControl
{
    [GeneratedRegex("\\d+", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex NumbersOnlyRegex();

    public EditSearchSettingView()
    {
        InitializeComponent();
    }

    public EditSearchSettingView(EditSearchSettingViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }


    public async Task InitializeDataContextAsync(int settingId, CancellationToken cancellationToken = default)
    {
        if (DataContext is not EditSearchSettingViewModel viewModel)
        {
            return;
        }

        await viewModel.InitializeAsync(settingId, cancellationToken).ConfigureAwait(true);
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
