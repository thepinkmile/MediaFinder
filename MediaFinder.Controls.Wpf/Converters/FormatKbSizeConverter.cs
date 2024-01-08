using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace MediaFinder.Controls.Wpf.Converters;

public class FormatKbSizeConverter : IValueConverter
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern long StrFormatByteSizeW(long qdw, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf,
        int cchBuf);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var number = System.Convert.ToInt64(value ?? 0);
        var sb = new StringBuilder(32);
        StrFormatByteSizeW(number, sb, sb.Capacity);
        return sb.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}