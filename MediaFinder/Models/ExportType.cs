using System.ComponentModel.DataAnnotations;

using NetEscapades.EnumGenerators;

namespace MediaFinder_v2.Models;

[EnumExtensions]
public enum ExportType
{
    [Display(Name = "Original Path")]
    OriginalPath,

    [Display(Name = "By Created Date")]
    ByDateCreated,

    [Display(Name = "By File Checksum")]
    ByChecksum,

    [Display(Name = "Flat Directory")]
    Flat
}
