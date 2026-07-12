using UBP.Results;

namespace UBP.Booking.Application.Errors;

public static class BookingErrors
{
    public const string NotFoundCode = "Booking.NotFound";
    public static readonly Error NotFound = new(NotFoundCode, "The booking was not found.");

    public const string SlotUnavailableCode = "Booking.SlotUnavailable";
    public static readonly Error SlotUnavailable = new(SlotUnavailableCode, "The requested slot has no remaining capacity.");

    public const string InvalidStatusTransitionCode = "Booking.InvalidStatusTransition";
    public static readonly Error InvalidStatusTransition = new(InvalidStatusTransitionCode, "The booking cannot transition from its current status.");

    public const string ReferenceGenerationFailedCode = "Booking.ReferenceGenerationFailed";
    public static readonly Error ReferenceGenerationFailed = new(ReferenceGenerationFailedCode, "Could not generate a unique booking reference.");
}
