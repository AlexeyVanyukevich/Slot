namespace UBP.Storage.Abstractions;

public class AssetStorageOptions
{
    public HashSet<string> AllowedExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public long MaxSizeBytes { get; set; }
}
