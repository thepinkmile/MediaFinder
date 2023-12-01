using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;
using System.Windows;

using Windows.Win32;

namespace MediaFinder.Controls.Wpf.Converters;

public class FormatKbSizeConverter : IValueConverter
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern long StrFormatByteSizeW(long qdw, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf,
        int cchBuf);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows only application")]
    public unsafe object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var buffer = new char[32];
        var number = System.Convert.ToInt64(value);
        fixed (char* pBuff = buffer)
        {
            _ = PInvoke.StrFormatByteSize(number, pBuff, 32);
        }

        return new string(buffer);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}