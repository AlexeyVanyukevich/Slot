using Microsoft.AspNetCore.Routing;

using UBP.Booking.Application.Commands;
using UBP.CQRS;
using UBP.Endpoints.Interfaces;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Booking.API.Endpoints;

internal sealed class CreateBookingEndpoint : IGroupEndpoint<BookingEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", async (CreateBookingRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(
                new CreateBookingCommand(request.SlotId, request.CustomerName, request.CustomerEmail, request.CustomerPhone, request.CustomerNotes, request.AutoConfirm),
                cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }).AllowAnonymous();
    }
}

internal sealed record CreateBookingRequest(Guid SlotId, string CustomerName, string CustomerEmail, string CustomerPhone, string? CustomerNotes, bool AutoConfirm = false);
