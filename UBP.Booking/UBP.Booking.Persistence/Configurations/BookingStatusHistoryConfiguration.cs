using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using UBP.Booking.Domain.Entities;
using UBP.Core.Data.EF.Configurations;

namespace UBP.Booking.Persistence.Configurations;

internal sealed class BookingStatusHistoryConfiguration() : EntityConfiguration<BookingStatusHistoryEntity, Guid>(TableName)
{
    public const string TableName = "booking_status_history";

    public override void Configure(EntityTypeBuilder<BookingStatusHistoryEntity> builder)
    {
        base.Configure(builder);

        builder.HasIndex(h => h.BookingId);
        builder.HasIndex(h => h.ChangedAt);

        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.ChangedByType).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.Reason).HasMaxLength(500);

        builder.HasOne<BookingEntity>()
            .WithMany()
            .HasForeignKey(h => h.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
