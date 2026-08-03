using System.ComponentModel.DataAnnotations;

using MediaFinder.Models;

namespace MediaFinder.DataAccessLayer.Models;

public class FileDetails
{
    public int Id { get; set; }

    public required string ParentPath { get; set; }

    public required string FileName { get; set; }

    [MaxLength(32)]
    public string? MD5_Hash { get; set; }

    [MaxLength(256)]
    public string? SHA256_Hash { get; set; }

    [MaxLength(512)]
    public string? SHA512_Hash { get; set; }

    public long FileSize { get; set; }

    public bool ShouldExport { get; set; }

    public MultiMediaType FileType { get; set; } = MultiMediaType.Unknown;

    public virtual ICollection<FileProperty> FileProperties { get; set; } = new List<FileProperty>();

    public bool Extracted { get; set; }

    public DateTimeOffset Created {  get; set; }

    public string RelativePath { get; set; } = null!;
}
