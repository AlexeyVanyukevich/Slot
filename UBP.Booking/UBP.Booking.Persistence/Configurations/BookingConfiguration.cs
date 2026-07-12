using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using UBP.Booking.Domain.Entities;
using UBP.Core.Data.EF.Configurations;

namespace UBP.Booking.Persistence.Configurations;

internal sealed class BookingConfiguration() : EntityConfiguration<BookingEntity, Guid>(TableName)
{
    public const string TableName = "bookings";

    public override void Configure(EntityTypeBuilder<BookingEntity> builder)
    {
        base.Configure(builder);

        builder.HasIndex(b => b.TenantId);
        builder.HasIndex(b => b.ResourceId);
        builder.HasIndex(b => b.ExternalReference).IsUnique();
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.CustomerEmail);

        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.ExternalReference).HasMaxLength(20).IsRequired();
        builder.Property(b => b.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.CustomerEmail).HasMaxLength(255).IsRequired();
        builder.Property(b => b.CustomerPhone).HasMaxLength(50).IsRequired();
        builder.Property(b => b.CancellationReason).HasMaxLength(500);

        builder.HasOne<AvailabilitySlotEntity>()
            .WithMany()
            .HasForeignKey(b => b.SlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
