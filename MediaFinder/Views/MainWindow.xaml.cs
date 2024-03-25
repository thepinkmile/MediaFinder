using System.ComponentModel;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

namespace MediaFinder.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly ILogger<MainWindow> _logger;

    public MainWindow(MainWindowViewModel mainWindowViewModel, ILogger<MainWindow> logger)
    {
        _logger = logger;
        DataContext = mainWindowViewModel;

        InitializeComponent();

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.DiscoveryViewModel.CleanupAsync().ConfigureAwait(true);
                viewModel.ExportViewModel.Cleanup();
            }
            base.OnClosing(e);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while closing MainWindow");
        }
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
