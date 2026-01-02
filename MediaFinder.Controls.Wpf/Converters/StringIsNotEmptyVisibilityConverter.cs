using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaFinder.Controls.Wpf.Converters
{
    public class StringIsNotEmptyVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is not null && value.ToString() == string.Empty
                ? Visibility.Hidden
                : Visibility.Visible;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
