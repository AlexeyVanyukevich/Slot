using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Slot.Domain.Enums;

using SlotEntity = Slot.Domain.Entities.Slot;

namespace Slot.Persistence.Configurations;

internal sealed class SlotConfiguration() : EntityConfiguration<SlotEntity>(TableName)
{
    const string TableName = "slots";

    public override void Configure(EntityTypeBuilder<SlotEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Status).HasMaxLength(16).HasConversion(new EnumToStringConverter<SlotStatus>());

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Slots)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ServiceType)
            .WithMany(x => x.Slots)
            .HasForeignKey(x => x.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Resource)
            .WithMany(x => x.Slots)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
