using UBP.Booking.Application.Errors;
using UBP.Booking.Domain.Entities;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Commands;

public record RemoveAvailabilitySlotCommand(Guid SlotId) : IRequest<Result>;

internal sealed class RemoveAvailabilitySlotCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RemoveAvailabilitySlotCommand, Result>
{
    public async Task<Result> HandleAsync(RemoveAvailabilitySlotCommand request, CancellationToken cancellationToken = default)
    {
        var slotRepository = unitOfWork.Repository<AvailabilitySlotEntity>();
        var slot = await slotRepository.GetSingleOrDefaultAsync(s => s.Id == request.SlotId, cancellationToken);
        if (slot is null)
            return Result.Failure(SlotErrors.NotFound);

        if (slot.BookedCount > 0)
            return Result.Failure(SlotErrors.HasActiveBookings);

        slotRepository.Delete(slot);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
