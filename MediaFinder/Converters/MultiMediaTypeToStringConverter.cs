using System.Globalization;
using System.Windows.Data;

using MediaFinder.DataAccessLayer.Models;

namespace MediaFinder.Converters;

public class MultiMediaTypeToStringConverter : IValueConverter
{
    private static Dictionary<string, MultiMediaType> DisplayNameMapper
        => MultiMediaTypeExtensions.GetValues()
            .ToDictionary(x => x.ToStringFast(), x => x);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not MultiMediaType mediaType
            ? Binding.DoNothing
            : mediaType.ToStringFast();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    => value is string name
            ? MultiMediaTypeExtensions.TryParse(name, out var newValue, true, true)
                ? newValue
                : DisplayNameMapper.ContainsKey(name)
                    ? DisplayNameMapper[name]
                    : Binding.DoNothing
            : Binding.DoNothing;
}
