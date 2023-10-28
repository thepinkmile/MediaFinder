using NetEscapades.EnumGenerators;

namespace MediaFinder_v2.DataAccessLayer.Models;

[Flags]
[EnumExtensions]
public enum MultiMediaType
{
    Unknown = 0,

    Image = 1,

    Video = 2,

    Audio = 4,

    PlayableMedia = Video | Audio
}
