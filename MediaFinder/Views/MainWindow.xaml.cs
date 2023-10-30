﻿using System.ComponentModel;
using System.Windows.Input;

namespace MediaFinder_v2.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(MainWindowsViewModel mainWindowViewModel)
    {
        DataContext = mainWindowViewModel;

        InitializeComponent();

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainWindowsViewModel viewModel)
        {
            await viewModel.DiscoveryViewModel.Cleanup();
            viewModel.ExportViewModel.Cleanup();
        }
        base.OnClosing(e);
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}