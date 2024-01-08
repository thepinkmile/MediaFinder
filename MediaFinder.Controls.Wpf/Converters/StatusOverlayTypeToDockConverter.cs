using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaFinder.Controls.Wpf.Converters;

public class StatusOverlayTypeToDockConverter : IValueConverter
{
    public Dock LinearPosition { get; set; }

    public Dock CircularPosition { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is StatusOverlayType type
            ? type switch
                {
                    StatusOverlayType.Linear => LinearPosition,
                    _ => CircularPosition,
                }
            : Binding.DoNothing;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
