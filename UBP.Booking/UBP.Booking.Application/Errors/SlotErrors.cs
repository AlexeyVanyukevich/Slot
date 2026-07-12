using UBP.Results;

namespace UBP.Booking.Application.Errors;

public static class SlotErrors
{
    public const string NotFoundCode = "Slot.NotFound";
    public static readonly Error NotFound = new(NotFoundCode, "The availability slot was not found.");

    public const string InvalidTimeRangeCode = "Slot.InvalidTimeRange";
    public static readonly Error InvalidTimeRange = new(InvalidTimeRangeCode, "The slot end time must be after the start time.");

    public const string InvalidCapacityCode = "Slot.InvalidCapacity";
    public static readonly Error InvalidCapacity = new(InvalidCapacityCode, "Capacity cannot be negative.");

    public const string HasActiveBookingsCode = "Slot.HasActiveBookings";
    public static readonly Error HasActiveBookings = new(HasActiveBookingsCode, "The slot cannot be removed while it has active bookings.");
}
