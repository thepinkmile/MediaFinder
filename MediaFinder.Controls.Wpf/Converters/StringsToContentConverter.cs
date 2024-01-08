using System.Globalization;
using System.Windows.Data;

namespace MediaFinder.Controls.Wpf.Converters;

public class StringsToContentConverter : IValueConverter
{
    public string Delimeter { get; set; } = ",";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not IEnumerable<string> list
            ? Binding.DoNothing
            : list != null && list.Any()
                ? string.Join(
                    parameter is string paramStr && !string.IsNullOrWhiteSpace(paramStr)
                        ? paramStr
                        : Delimeter
                    , list)
                : (object)string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not string strValue
            ? Binding.DoNothing
            : strValue.Split(Delimeter, StringSplitOptions.RemoveEmptyEntries);
}
