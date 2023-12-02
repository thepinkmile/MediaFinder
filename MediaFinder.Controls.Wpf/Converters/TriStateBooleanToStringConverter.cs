using MediaFinder.Helpers;

using System.Globalization;
using System.Windows.Data;

namespace MediaFinder.Controls.Wpf.Converters;

public class TriStateBooleanToStringConverter : IValueConverter
{
    private static Dictionary<string, TriStateBoolean> DisplayNameMapper
       => TriStateBooleanExtensions.GetValues()
           .ToDictionary(x => x.ToStringFast(), x => x);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not TriStateBoolean mediaType
            ? Binding.DoNothing
            : mediaType.ToStringFast();

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    => value is string name
            ? TriStateBooleanExtensions.TryParse(name, out var newValue, true, true)
                ? newValue
                : DisplayNameMapper.TryGetValue(name, out var triValue)
                    ? triValue
                    : Binding.DoNothing
            : Binding.DoNothing;
}
