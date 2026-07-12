using UBP.Booking.Application.Dtos;
using UBP.Booking.Application.Errors;
using UBP.Booking.Domain.Entities;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Commands;

public record CreateAvailabilitySlotCommand(Guid TenantId, Guid ResourceId, DateTime StartAt, DateTime EndAt, int Capacity = 1) : IRequest<Result<SlotDto>>;

internal sealed class CreateAvailabilitySlotCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateAvailabilitySlotCommand, Result<SlotDto>>
{
    public async Task<Result<SlotDto>> HandleAsync(CreateAvailabilitySlotCommand request, CancellationToken cancellationToken = default)
    {
        if (request.EndAt <= request.StartAt)
            return Result.Failure<SlotDto>(SlotErrors.InvalidTimeRange);

        if (request.Capacity < 0)
            return Result.Failure<SlotDto>(SlotErrors.InvalidCapacity);

        var slot = new AvailabilitySlotEntity
        {
            Id = Guid.CreateVersion7(),
            TenantId = request.TenantId,
            ResourceId = request.ResourceId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Capacity = request.Capacity
        };

        unitOfWork.Repository<AvailabilitySlotEntity>().Add(slot);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(SlotDto.FromEntity(slot));
    }
}
