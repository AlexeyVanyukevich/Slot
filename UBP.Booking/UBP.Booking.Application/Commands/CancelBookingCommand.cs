using UBP.Booking.Application.Errors;
using UBP.Booking.Domain.Entities;
using UBP.Booking.Domain.Enums;
using UBP.Booking.Persistence.Interfaces;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Commands;

public record CancelBookingCommand(Guid BookingId, string? Reason, ChangedByType ChangedByType) : IRequest<Result>;

internal sealed class CancelBookingCommandHandler(IUnitOfWork unitOfWork, IAvailabilitySlotRepository slotRepository)
    : IRequestHandler<CancelBookingCommand, Result>
{
    public async Task<Result> HandleAsync(CancelBookingCommand request, CancellationToken cancellationToken = default)
    {
        var bookingRepository = unitOfWork.Repository<BookingEntity>();
        var booking = await bookingRepository.GetSingleOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(BookingErrors.NotFound);

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed or BookingStatus.NoShow)
            return Result.Failure(BookingErrors.InvalidStatusTransition);

        var previousStatus = booking.Status;
        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = request.Reason;
        bookingRepository.Update(booking);

        unitOfWork.Repository<BookingStatusHistoryEntity>().Add(new BookingStatusHistoryEntity
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            FromStatus = previousStatus,
            ToStatus = BookingStatus.Cancelled,
            ChangedByType = request.ChangedByType,
            Reason = request.Reason,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await slotRepository.ReleaseAsync(booking.SlotId, cancellationToken);

        return Result.Success();
    }
}
