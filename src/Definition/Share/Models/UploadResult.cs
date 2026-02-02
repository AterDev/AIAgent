using Perigon.AspNetCore.Options;

namespace Share.Models;

public class UploadResult
{
    public string? FilePath { get; set; }
    public string? Url { get; set; }
    public StorageType StorageType { get; set; }
}
