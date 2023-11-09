using System.Globalization;
using System.Windows.Data;

namespace MediaFinder.Converters;

public class DateTimeOffsetToStringConverter : IValueConverter
{
    public string DateFormat { get; set; } = "r";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not DateTimeOffset dtoValue
            ? Binding.DoNothing
            : dtoValue.ToString(DateFormat, CultureInfo.InvariantCulture);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not string strValue
            || !DateTimeOffset.TryParseExact(strValue, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal, out var dtoValue)
                ? Binding.DoNothing
                : dtoValue;
}
