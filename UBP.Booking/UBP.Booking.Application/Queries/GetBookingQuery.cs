using UBP.Booking.Application.Dtos;
using UBP.Booking.Application.Errors;
using UBP.Booking.Domain.Entities;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Queries;

public record GetBookingQuery(Guid BookingId) : IRequest<Result<BookingDto>>;

internal sealed class GetBookingQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetBookingQuery, Result<BookingDto>>
{
    public async Task<Result<BookingDto>> HandleAsync(GetBookingQuery request, CancellationToken cancellationToken = default)
    {
        var booking = await unitOfWork.Repository<BookingEntity>().GetSingleOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);
        return booking is null
            ? Result.Failure<BookingDto>(BookingErrors.NotFound)
            : Result.Success(BookingDto.FromEntity(booking));
    }
}
