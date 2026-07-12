using Microsoft.AspNetCore.Routing;

using UBP.Booking.Application.Commands;
using UBP.CQRS;
using UBP.Endpoints.Interfaces;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Booking.API.Endpoints;

internal sealed class ConfirmBookingEndpoint : IGroupEndpoint<BookingEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{id:guid}/confirm", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(new ConfirmBookingCommand(id), cancellationToken);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }).RequireAuthorization();
    }
}
