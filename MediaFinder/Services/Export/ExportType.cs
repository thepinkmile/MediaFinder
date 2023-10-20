using System.ComponentModel.DataAnnotations;

namespace MediaFinder_v2.Services.Export;

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
