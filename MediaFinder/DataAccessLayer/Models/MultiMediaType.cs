using NetEscapades.EnumGenerators;

namespace MediaFinder_v2.DataAccessLayer.Models;

[EnumExtensions]
public enum MultiMediaType
{
    Unknown,

    Image,

    Video,

    Audio
}
