using UBP.Core.Entities;

namespace UBP.Storage.Domain.Entities;

public class AssetEntity : Entity<Guid>
{
    public required string StorageKey { get; set; }
    public required string ContentType { get; set; }
    public required long SizeBytes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
