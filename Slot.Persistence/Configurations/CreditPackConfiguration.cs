using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Slot.Domain.Entities;

namespace Slot.Persistence.Configurations;

internal sealed class CreditPackConfiguration() : EntityConfiguration<CreditPack>(TableName)
{
    const string TableName = "credit_packs";

    public override void Configure(EntityTypeBuilder<CreditPack> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.CreditPacks)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.CreditPacks)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CustomerId, x.IsFrozen, x.ValidUntil });
    }
}
