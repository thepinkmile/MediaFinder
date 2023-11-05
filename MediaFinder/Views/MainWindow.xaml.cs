﻿using System.ComponentModel;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

namespace MediaFinder_v2.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly ILogger<MainWindow> _logger;

    public MainWindow(MainWindowsViewModel mainWindowViewModel, ILogger<MainWindow> logger)
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
            if (DataContext is MainWindowsViewModel viewModel)
            {

#pragma warning disable CRR0039
                await viewModel.DiscoveryViewModel.CleanupAsync().ConfigureAwait(true);
#pragma warning restore CRR0039
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
