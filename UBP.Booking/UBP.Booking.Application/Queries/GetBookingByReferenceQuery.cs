using UBP.Booking.Application.Dtos;
using UBP.Booking.Application.Errors;
using UBP.Booking.Domain.Entities;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Queries;

public record GetBookingByReferenceQuery(string ExternalReference) : IRequest<Result<BookingDto>>;

internal sealed class GetBookingByReferenceQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetBookingByReferenceQuery, Result<BookingDto>>
{
    public async Task<Result<BookingDto>> HandleAsync(GetBookingByReferenceQuery request, CancellationToken cancellationToken = default)
    {
        var booking = await unitOfWork.Repository<BookingEntity>()
            .GetSingleOrDefaultAsync(b => b.ExternalReference == request.ExternalReference, cancellationToken);

        return booking is null
            ? Result.Failure<BookingDto>(BookingErrors.NotFound)
            : Result.Success(BookingDto.FromEntity(booking));
    }
}
