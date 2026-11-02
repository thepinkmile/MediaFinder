using System.Windows.Controls;

using MediaFinder.Models;

namespace MediaFinder.Views.Export;

/// <summary>
/// Interaction logic for ImageViewerView.xaml
/// </summary>
public partial class ImageViewerView : UserControl
{
    public ImageViewerView()
    {
        InitializeComponent();
    }

    public ImageViewerView(MediaFile viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
