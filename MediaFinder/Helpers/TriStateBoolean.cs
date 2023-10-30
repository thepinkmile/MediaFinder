using NetEscapades.EnumGenerators;

namespace MediaFinder_v2.Helpers;

[Flags]
[EnumExtensions]
public enum TriStateBoolean
{
    None = 0,
    False = 1,
    True = 2,
    All = ~(~0 << 2)
}
