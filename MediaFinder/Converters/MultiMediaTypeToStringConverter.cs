using System.Globalization;
using System.Windows.Data;

using MediaFinder_v2.DataAccessLayer.Models;

namespace MediaFinder_v2.Converters;

public class MultiMediaTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not MultiMediaType mediaType
            ? Binding.DoNothing
            : mediaType switch
            {
                MultiMediaType.Image => "Image",
                MultiMediaType.Video => "Video",
                MultiMediaType.Audio => "Audio",
                _ => string.Empty
            };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
