using System.Globalization;
using System.Windows.Data;

using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Models;

namespace MediaFinder_v2.Converters;

public class ExportTypeToStringConverter : IValueConverter
{
    private static Dictionary<string, ExportType> DisplayNameMapper
        => ExportTypeExtensions.GetValues()
            .ToDictionary(x => x.ToStringFast(), x => x);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not ExportType mediaType
            ? Binding.DoNothing
            : mediaType.ToStringFast();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string name
            ? ExportTypeExtensions.TryParse(name, out var newValue, true, true)
                ? newValue
                : DisplayNameMapper.ContainsKey(name)
                    ? DisplayNameMapper[name]
                    : Binding.DoNothing
            : Binding.DoNothing;
}
