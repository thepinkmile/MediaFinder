using System.Globalization;
using System.Windows.Data;

using Windows.Win32;

namespace MediaFinder.Controls.Wpf.Converters;

public class FormatKbSizeConverter : IValueConverter
{
    public unsafe object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not { } actualValue
            ? Binding.DoNothing
            : actualValue switch
            {
                short shortValue => PInvoke.StrFormatByteSize(shortValue),
                ushort ushortValue => PInvoke.StrFormatByteSize(ushortValue),
                int intValue => PInvoke.StrFormatByteSize(intValue),
                uint uintValue => PInvoke.StrFormatByteSize(uintValue),
                long longValue => PInvoke.StrFormatByteSize(longValue),
                ulong ulongValue => PInvoke.StrFormatByteSize(ulongValue),
                Int128 llongValue => PInvoke.StrFormatByteSize(llongValue),
                UInt128 ullongValue => PInvoke.StrFormatByteSize(ullongValue),
                _ => Binding.DoNothing,
            };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}