using System.Globalization;
using System.Windows.Data;

namespace MediaFinder.Controls.Wpf.Converters;

public class StatusOverlayTypeToSizeConverter : IValueConverter
{
    public double? LinearSize { get; set; }

    public double? CircularSize { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is StatusOverlayType type
            ? type switch
                {
                    StatusOverlayType.Linear => LinearSize,
                    _ => CircularSize,
                }
            : Binding.DoNothing;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
