using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using UBP.Booking.Domain.Entities;
using UBP.Core.Data.EF.Configurations;

namespace UBP.Booking.Persistence.Configurations;

internal sealed class AvailabilitySlotConfiguration() : EntityConfiguration<AvailabilitySlotEntity, Guid>(TableName)
{
    public const string TableName = "availability_slots";

    public override void Configure(EntityTypeBuilder<AvailabilitySlotEntity> builder)
    {
        base.Configure(builder);

        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => new { s.ResourceId, s.StartAt });
    }
}
