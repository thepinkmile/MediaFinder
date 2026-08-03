using System.ComponentModel.DataAnnotations;

using NetEscapades.EnumGenerators;

namespace MediaFinder.Models;

[Flags]
[EnumExtensions]
public enum MultiMediaType
{
    None = 0,

    Unknown = 1,

    Image = 2,

    Video = 4,

    Audio = 8,

    [Display(Name = "Playable")]
    PlayableMedia = Video | Audio,

    [Display(Name = "Media")]
    MultiMedia = Video | Audio | Image,

    All = ~(~0 << 4)
}
