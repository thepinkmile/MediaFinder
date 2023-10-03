namespace MediaFinder_v2.DataAccessLayer.Models;

public class FileDetails
{
    public int Id { get; set; }

    public string ParentPath { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string MD5_Hash { get; set; } = null!;

    public string SHA256_Hash { get; set; } = null!;

    public string SHA512_Hash { get; set; } = null!;

    public long FileSize { get; set; }

    public bool ShouldExport { get; set; } = true;

    public MultiMediaType FileType { get; set; }

    public virtual ICollection<FileProperty> FileProperties { get; set; } = new List<FileProperty>();
}
