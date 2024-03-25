using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaFinder.Controls.Wpf.Converters;

public class StatusOverlayTypeToStyleConverter : IValueConverter
{
    public Style? LinearStyle { get; set; }

    public Style? CircularStyle { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is StatusOverlayType type
            ? type switch
                {
                    StatusOverlayType.Linear => LinearStyle,
                    _ => CircularStyle,
                }
            : Binding.DoNothing;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
