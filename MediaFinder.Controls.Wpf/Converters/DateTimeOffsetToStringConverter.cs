using System.Globalization;
using System.Windows.Data;

namespace MediaFinder.Controls.Wpf.Converters;

public class DateTimeOffsetToStringConverter : IValueConverter
{
    public string DateFormat { get; set; } = "r";

    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    public DateTimeStyles Styles { get; set; } = DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not DateTimeOffset dtoValue
            ? Binding.DoNothing
            : dtoValue.ToString(DateFormat, Culture);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not string strValue
            || !DateTimeOffset.TryParseExact(strValue, DateFormat, Culture, Styles, out var dtoValue)
                ? Binding.DoNothing
                : dtoValue;
}
