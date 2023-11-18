using NetEscapades.EnumGenerators;

namespace MediaFinder.Helpers;

[Flags]
[EnumExtensions]
public enum TriStateBoolean
{
    None = 0,
    False = 1,
    True = 2,
    All = ~(~0 << 2)
}
