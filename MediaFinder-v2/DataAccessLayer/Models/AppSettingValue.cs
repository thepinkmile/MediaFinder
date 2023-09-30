using System.ComponentModel.DataAnnotations;

namespace MediaFinder_v2.DataAccessLayer.Models;

public class AppSettingValue
{
    [Key]
    public required string Key { get; set; }

    public required string Value { get; set; }
}
