using UBP.Booking.Domain.Enums;
using UBP.Core.Entities;

namespace UBP.Booking.Domain.Entities;

public class BookingStatusHistoryEntity : Entity<Guid>
{
    public required Guid BookingId { get; set; }
    public BookingStatus? FromStatus { get; set; }
    public required BookingStatus ToStatus { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public required ChangedByType ChangedByType { get; set; }
    public string? Reason { get; set; }
    public required DateTimeOffset ChangedAt { get; set; }
}
