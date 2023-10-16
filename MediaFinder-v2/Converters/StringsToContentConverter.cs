using System.Globalization;
using System.Windows.Data;

namespace MediaFinder_v2.Converters;

public class StringsToContentConverter : IValueConverter
{
    public string Delimeter { get; set; } = ",";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<string> list)
        {
            return Binding.DoNothing;
        }
        
        if (parameter != null && parameter is string paramStr)
        {
            if (!string.IsNullOrWhiteSpace(paramStr))
            {
                Delimeter = paramStr;
            }
        }

        return list != null && list.Any()
            ? string.Join(Delimeter, list)
            : (object)string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not string strValue
            ? Binding.DoNothing
            : strValue.Split(Delimeter, StringSplitOptions.RemoveEmptyEntries);
}
