using MaterialDesignThemes.Wpf.Converters;

namespace MediaFinder.Converters;

public class BooleanToStringConverter : BooleanConverter<string>
{
    public BooleanToStringConverter() : base("True", "False")
    {
    }
}
