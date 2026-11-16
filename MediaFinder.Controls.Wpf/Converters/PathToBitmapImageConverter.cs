using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MediaFinder.Controls.Wpf.Converters;

public class PathToBitmapImageConverter : IValueConverter
{
    public int? DecodePixelWidth { get; set; } = null;

    public int? DecodePixelHeight { get; set; } = null;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || path is null || !File.Exists(path))
        {
            return Binding.DoNothing;
        }

        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            if (DecodePixelWidth is not null)
            {
                bmp.DecodePixelWidth = DecodePixelWidth.Value;
            }
            if (DecodePixelHeight is not null)
            {
                bmp.DecodePixelHeight = DecodePixelHeight.Value;
            }
            bmp.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bmp.EndInit();
            return bmp;
        }
        catch
        {
            return Binding.DoNothing;
        }
    }


    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
