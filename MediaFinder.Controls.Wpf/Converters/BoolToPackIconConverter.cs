using System.Globalization;
using System.Windows.Data;

using MaterialDesignThemes.Wpf;

namespace MediaFinder.Controls.Wpf.Converters;

public class BoolToPackIconConverter : IValueConverter
{
    public PackIconKind TrueValue { get; set; } = PackIconKind.CheckboxMarked;

    public PackIconKind FalseValue { get; set; } = PackIconKind.CheckboxBlank;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool bVal
            ? bVal
                ? TrueValue
                : FalseValue
            : Binding.DoNothing;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
