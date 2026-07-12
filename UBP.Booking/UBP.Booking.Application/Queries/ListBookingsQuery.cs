using UBP.Booking.Application.Dtos;
using UBP.Booking.Domain.Entities;
using UBP.Booking.Domain.Enums;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Queries;

public record ListBookingsQuery(
    Guid? ResourceId,
    Guid? TenantId,
    BookingStatus? Status,
    DateTime? FromUtc,
    DateTime? ToUtc,
    string? CustomerSearch) : IRequest<Result<IReadOnlyList<BookingDto>>>;

internal sealed class ListBookingsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListBookingsQuery, Result<IReadOnlyList<BookingDto>>>
{
    public async Task<Result<IReadOnlyList<BookingDto>>> HandleAsync(ListBookingsQuery request, CancellationToken cancellationToken = default)
    {
        var bookings = await unitOfWork.Repository<BookingEntity>().GetAsync(b =>
            (request.ResourceId == null || b.ResourceId == request.ResourceId) &&
            (request.TenantId == null || b.TenantId == request.TenantId) &&
            (request.Status == null || b.Status == request.Status) &&
            (request.FromUtc == null || b.SlotStartAt >= request.FromUtc) &&
            (request.ToUtc == null || b.SlotStartAt <= request.ToUtc) &&
            (request.CustomerSearch == null || b.CustomerName.Contains(request.CustomerSearch) || b.CustomerEmail.Contains(request.CustomerSearch)),
            cancellationToken);

        IReadOnlyList<BookingDto> dtos = bookings
            .OrderByDescending(b => b.CreatedAt)
            .Select(BookingDto.FromEntity)
            .ToList();

        return Result.Success(dtos);
    }
}
