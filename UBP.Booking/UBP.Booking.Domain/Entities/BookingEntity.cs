using UBP.Booking.Domain.Enums;
using UBP.Core.Entities;
using UBP.Core.Interfaces;

namespace UBP.Booking.Domain.Entities;

public class BookingEntity : Entity<Guid>, IAuditable
{
    public required Guid SlotId { get; set; }
    public required Guid TenantId { get; set; }
    public required Guid ResourceId { get; set; }
    public required DateTime SlotStartAt { get; set; }
    public required DateTime SlotEndAt { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public required string CustomerPhone { get; set; }
    public string? CustomerNotes { get; set; }
    public required string ExternalReference { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}
