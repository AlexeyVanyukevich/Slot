using Microsoft.EntityFrameworkCore.Metadata.Builders;

using UBP.Core.Data.EF.Configurations;
using UBP.Storage.Domain.Entities;

namespace UBP.Storage.Persistence.Configurations;

internal sealed class AssetConfiguration() : EntityConfiguration<AssetEntity, Guid>(TableName)
{
    public const string TableName = "assets";

    public override void Configure(EntityTypeBuilder<AssetEntity> builder)
    {
        base.Configure(builder);

        builder.Property(a => a.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(a => a.ContentType).HasMaxLength(100).IsRequired();
    }
}
