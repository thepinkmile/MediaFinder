using System.Globalization;
using System.Windows.Data;

using MediaFinder_v2.Models;

namespace MediaFinder_v2.Converters;

public class MediaFilePropertyConverter : IValueConverter
{
    public string PropertyName { get; set; } = null!;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not MediaFile mf
            ? Binding.DoNothing
            : mf.Properties.ContainsKey(PropertyName)
                ? mf.Properties[PropertyName]
                : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
