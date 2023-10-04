using MaterialDesignThemes.Wpf.Converters;

namespace MediaFinder_v2.Converters;

public class BooleanToStringConverter : BooleanConverter<string>
{
    public BooleanToStringConverter() : base("True", "False")
    {
    }
}
