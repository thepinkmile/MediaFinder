using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Data;

namespace MediaFinder_v2.Converters;

public class PercentageConverter : IValueConverter
{
    [Range(0, 100)]
    public required int Percentage { get; set; } = 100;

    private double ActualPercentage => (double)Percentage / 100;

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is null
            ? null
            : (double?)value * ActualPercentage;

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is null
            ? null
            : (double?)value / ActualPercentage;
}