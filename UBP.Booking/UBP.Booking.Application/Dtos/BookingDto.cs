using UBP.Booking.Domain.Entities;
using UBP.Booking.Domain.Enums;

namespace UBP.Booking.Application.Dtos;

public sealed record BookingDto(
    Guid Id,
    Guid SlotId,
    Guid TenantId,
    Guid ResourceId,
    DateTime SlotStartAt,
    DateTime SlotEndAt,
    BookingStatus Status,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string? CustomerNotes,
    string ExternalReference,
    DateTimeOffset CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CancelledAt,
    string? CancellationReason)
{
    public static BookingDto FromEntity(BookingEntity booking) => new(
        booking.Id, booking.SlotId, booking.TenantId, booking.ResourceId,
        booking.SlotStartAt, booking.SlotEndAt, booking.Status,
        booking.CustomerName, booking.CustomerEmail, booking.CustomerPhone, booking.CustomerNotes,
        booking.ExternalReference, booking.CreatedAt, booking.ConfirmedAt, booking.CancelledAt, booking.CancellationReason);
}
