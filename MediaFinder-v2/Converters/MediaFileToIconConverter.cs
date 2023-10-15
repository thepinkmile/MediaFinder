using System.Globalization;
using System.Windows.Data;

using MaterialDesignThemes.Wpf;

using MediaFinder_v2.Views.Executors;

namespace MediaFinder_v2.Converters;

public class MediaFileToIconConverter : IValueConverter
{
    public PackIconKind ImageIcon { get; set; } = PackIconKind.ImageArea;

    public PackIconKind VideoIcon { get; set; } = PackIconKind.VideoImage;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not MediaFile mf ? (object)PackIconKind.None
            : mf.IsImage ? ImageIcon
            : mf.IsVideo ? VideoIcon
            : (object)PackIconKind.None;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
