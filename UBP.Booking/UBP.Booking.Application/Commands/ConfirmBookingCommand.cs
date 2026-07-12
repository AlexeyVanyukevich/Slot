using UBP.Booking.Application.Errors;
using UBP.Booking.Domain.Entities;
using UBP.Booking.Domain.Enums;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Commands;

public record ConfirmBookingCommand(Guid BookingId) : IRequest<Result>;

internal sealed class ConfirmBookingCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ConfirmBookingCommand, Result>
{
    public async Task<Result> HandleAsync(ConfirmBookingCommand request, CancellationToken cancellationToken = default)
    {
        var bookingRepository = unitOfWork.Repository<BookingEntity>();
        var booking = await bookingRepository.GetSingleOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(BookingErrors.NotFound);

        if (booking.Status != BookingStatus.Pending)
            return Result.Failure(BookingErrors.InvalidStatusTransition);

        var previousStatus = booking.Status;
        booking.Status = BookingStatus.Confirmed;
        booking.ConfirmedAt = DateTime.UtcNow;
        bookingRepository.Update(booking);

        unitOfWork.Repository<BookingStatusHistoryEntity>().Add(new BookingStatusHistoryEntity
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            FromStatus = previousStatus,
            ToStatus = BookingStatus.Confirmed,
            ChangedByType = ChangedByType.TenantUser,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
