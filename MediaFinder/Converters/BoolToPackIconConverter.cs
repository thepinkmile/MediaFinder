using System.Globalization;
using System.Windows.Data;

using MaterialDesignThemes.Wpf;

namespace MediaFinder.Converters;

public class BoolToPackIconConverter : IValueConverter
{
    public PackIconKind TrueValue { get; set; } = PackIconKind.CheckboxMarked;

    public PackIconKind FalseValue { get; set; } = PackIconKind.CheckboxBlank;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not bool bVal
            ? Binding.DoNothing
            : bVal
                ? TrueValue
                : FalseValue;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
