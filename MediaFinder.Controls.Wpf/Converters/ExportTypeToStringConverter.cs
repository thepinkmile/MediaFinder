using System.Globalization;
using System.Windows.Data;

using MediaFinder.Models;

namespace MediaFinder.Controls.Wpf.Converters;

public class ExportTypeToStringConverter : IValueConverter
{
    private static Dictionary<string, ExportType> DisplayNameMapper
        => ExportTypeExtensions.GetValues()
            .ToDictionary(x => x.ToStringFast(), x => x);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not ExportType exportType
            ? Binding.DoNothing
            : exportType.ToStringFast();

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string name
            ? ExportTypeExtensions.TryParse(name, out var newValue, true, true)
                ? newValue
                : DisplayNameMapper.TryGetValue(name, out var exportType)
                    ? exportType
                    : Binding.DoNothing
            : Binding.DoNothing;
}
