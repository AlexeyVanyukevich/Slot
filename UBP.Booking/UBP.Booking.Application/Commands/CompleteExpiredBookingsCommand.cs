using UBP.Booking.Domain.Entities;
using UBP.Booking.Domain.Enums;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Commands;

public record CompleteExpiredBookingsCommand : IRequest<Result<int>>;

internal sealed class CompleteExpiredBookingsCommandHandler(IUnitOfWork unitOfWork, TimeProvider? timeProvider = null)
    : IRequestHandler<CompleteExpiredBookingsCommand, Result<int>>
{
    private TimeProvider TimeProvider => timeProvider ?? TimeProvider.System;

    public async Task<Result<int>> HandleAsync(CompleteExpiredBookingsCommand request, CancellationToken cancellationToken = default)
    {
        var now = TimeProvider.GetUtcNow().UtcDateTime;
        var bookingRepository = unitOfWork.Repository<BookingEntity>();

        var expiredBookings = await bookingRepository.GetAsync(
            b => b.Status == BookingStatus.Confirmed && b.SlotEndAt <= now,
            cancellationToken);

        foreach (var booking in expiredBookings)
        {
            booking.Status = BookingStatus.Completed;
            bookingRepository.Update(booking);

            unitOfWork.Repository<BookingStatusHistoryEntity>().Add(new BookingStatusHistoryEntity
            {
                Id = Guid.CreateVersion7(),
                BookingId = booking.Id,
                FromStatus = BookingStatus.Confirmed,
                ToStatus = BookingStatus.Completed,
                ChangedByType = ChangedByType.System,
                ChangedAt = DateTimeOffset.UtcNow
            });
        }

        if (expiredBookings.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(expiredBookings.Count);
    }
}
