using System.Globalization;
using System.Windows;
using System.Windows.Data;

using MediaFinder_v2.DataAccessLayer.Models;

namespace MediaFinder_v2.Converters;

public class MultiMediaTypeToVisibilityConverter : IValueConverter
{
    public MultiMediaType RequiredType { get; set; }

    public Visibility VisibleStatae { get; set; } = Visibility.Visible;

    public Visibility InvisibleState { get; set; } = Visibility.Collapsed;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not MultiMediaType mediaType
            ? Binding.DoNothing
            : RequiredType.HasFlagFast(mediaType)
                ? VisibleStatae
                : InvisibleState;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
