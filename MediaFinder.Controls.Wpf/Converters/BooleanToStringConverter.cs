using MaterialDesignThemes.Wpf.Converters;

namespace MediaFinder.Controls.Wpf.Converters;

public class BooleanToStringConverter : BooleanConverter<string>
{
    public BooleanToStringConverter() : base("True", "False")
    {
    }
}
