using System.Globalization;
using System.Windows.Data;

namespace MediaFinder.Controls.Wpf.Converters;

public class NullableLongToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not long lngValue
            ? Binding.DoNothing
            : lngValue.ToString(CultureInfo.InvariantCulture);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not string strValue
            ? Binding.DoNothing
            : string.IsNullOrEmpty(strValue) || !long.TryParse(strValue, out var parsedValue)
                ? Binding.DoNothing
                : parsedValue;
}
