using System.Windows.Markup;

namespace MediaFinder_v2.Helpers;

public class EnumBindingSourceExtension : MarkupExtension
{
    public Type EnumType {  get; private set; }

    public EnumBindingSourceExtension(Type enumType)
    {
        if (enumType is null || !enumType.IsEnum)
            throw new ArgumentException("Argument was not an enum type.", nameof(enumType));

        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Enum.GetValues(EnumType);
    }
}
