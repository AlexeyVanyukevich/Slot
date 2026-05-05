using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Slot.Domain.Entities;
using Slot.Domain.Enums;

namespace Slot.Persistence.Configurations;

internal sealed class BookingConfiguration() : EntityConfiguration<Booking>(TableName)
{
    const string TableName = "bookings";

    public override void Configure(EntityTypeBuilder<Booking> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Status).HasMaxLength(32).HasConversion(new EnumToStringConverter<BookingStatus>());

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Slot)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.SlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
