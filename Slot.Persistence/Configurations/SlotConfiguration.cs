using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Slot.Domain.Entities;
using Slot.Domain.Enums;

namespace Slot.Persistence.Configurations;

internal sealed class SlotConfiguration() : EntityConfiguration<SlotEntity>(TableName)
{
    const string TableName = "slots";

    public override void Configure(EntityTypeBuilder<SlotEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Status).HasMaxLength(16).HasConversion(new EnumToStringConverter<SlotStatus>());

        builder.HasMany(x => x.Resources)
            .WithOne(x => x.Slot)
            .HasForeignKey(x => x.SlotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ServiceType)
            .WithMany(x => x.Slots)
            .HasForeignKey(x => x.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
