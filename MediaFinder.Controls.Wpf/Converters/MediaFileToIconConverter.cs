using System.Globalization;
using System.Windows.Data;

using MaterialDesignThemes.Wpf;

using MediaFinder.Models;

namespace MediaFinder.Controls.Wpf.Converters;

public class MediaFileToIconConverter : IValueConverter
{
    public PackIconKind ImageIcon { get; set; } = PackIconKind.FileImageOutline;

    public PackIconKind VideoIcon { get; set; } = PackIconKind.FileVideoOutline;

    public PackIconKind AudioIcon { get; set; } = PackIconKind.FileMusicOutline;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not MediaFile mf
            ? PackIconKind.None
            : mf.MultiMediaType switch
            {
                MultiMediaType.Video => VideoIcon,
                MultiMediaType.Audio => AudioIcon,
                MultiMediaType.Image => ImageIcon,
                _ => (object)PackIconKind.None
            };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
