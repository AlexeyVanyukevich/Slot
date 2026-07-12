using Microsoft.AspNetCore.Routing;

using UBP.Booking.Application.Queries;
using UBP.CQRS;
using UBP.Endpoints.Interfaces;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Booking.API.Endpoints;

internal sealed class GetBookingByReferenceEndpoint : IGroupEndpoint<BookingEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/by-reference/{reference}", async (string reference, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(new GetBookingByReferenceQuery(reference), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }).AllowAnonymous();
    }
}
