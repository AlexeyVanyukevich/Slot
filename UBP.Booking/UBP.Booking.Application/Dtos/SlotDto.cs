using UBP.Booking.Domain.Entities;

namespace UBP.Booking.Application.Dtos;

public sealed record SlotDto(
    Guid Id,
    Guid TenantId,
    Guid ResourceId,
    DateTime StartAt,
    DateTime EndAt,
    int Capacity,
    int BookedCount)
{
    public static SlotDto FromEntity(AvailabilitySlotEntity slot) => new(
        slot.Id, slot.TenantId, slot.ResourceId, slot.StartAt, slot.EndAt, slot.Capacity, slot.BookedCount);
}
