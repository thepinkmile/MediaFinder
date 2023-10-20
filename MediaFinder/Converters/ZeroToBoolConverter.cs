using System.Globalization;
using System.Windows.Data;

namespace MediaFinder_v2.Converters;

public class ZeroToBoolConverter : IValueConverter
{
    public bool TrueValue { get; set; } = true;

    public bool FalseValue { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (double.TryParse((value ?? "")!.ToString(), out var result))
        {
            return Math.Abs(result) > 0.0
                ? FalseValue
                : TrueValue;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
