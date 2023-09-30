using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MediaFinder_v2.Converters;

public class BoolToValueConverter<T> : IValueConverter
{
    public required T FalseValue { get; set; }
    public required T TrueValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => value is null
            ? (object)FalseValue!
            : (bool)value ? TrueValue! : FalseValue!;

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => value != null && value.Equals(TrueValue);
}

public class BoolToStringConverter : BoolToValueConverter<string> { }
public class BoolToBrushConverter : BoolToValueConverter<Brush> { }
public class BoolToVisibilityConverter : BoolToValueConverter<Visibility> { }
public class BoolToObjectConverter : BoolToValueConverter<object> { }