using UBP.Booking.Application.Dtos;
using UBP.Booking.Domain.Entities;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Queries;

public record GetAvailableSlotsQuery(Guid ResourceId, DateTime FromUtc, DateTime ToUtc) : IRequest<Result<IReadOnlyList<SlotDto>>>;

internal sealed class GetAvailableSlotsQueryHandler(IUnitOfWork unitOfWork, TimeProvider? timeProvider = null)
    : IRequestHandler<GetAvailableSlotsQuery, Result<IReadOnlyList<SlotDto>>>
{
    private TimeProvider TimeProvider => timeProvider ?? TimeProvider.System;

    public async Task<Result<IReadOnlyList<SlotDto>>> HandleAsync(GetAvailableSlotsQuery request, CancellationToken cancellationToken = default)
    {
        var now = TimeProvider.GetUtcNow().UtcDateTime;

        var slots = await unitOfWork.Repository<AvailabilitySlotEntity>().GetAsync(
            s => s.ResourceId == request.ResourceId
                && s.StartAt >= request.FromUtc
                && s.StartAt <= request.ToUtc
                && s.StartAt >= now
                && s.BookedCount < s.Capacity,
            cancellationToken);

        IReadOnlyList<SlotDto> dtos = slots
            .OrderBy(s => s.StartAt)
            .Select(SlotDto.FromEntity)
            .ToList();

        return Result.Success(dtos);
    }
}
