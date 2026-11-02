using System.Globalization;
using System.Windows.Data;

using MediaFinder.Models;

namespace MediaFinder.Controls.Wpf.Converters;

public class MediaFilePropertyConverter : IValueConverter
{
    public string PropertyName { get; set; } = null!;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not MediaFile mf || mf.Properties is not { }
            ? Binding.DoNothing
            : mf.Properties.TryGetValue(PropertyName, out var propValue)
                ? propValue
                : string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
