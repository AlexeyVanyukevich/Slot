using Microsoft.AspNetCore.Routing;

using UBP.Booking.Application.Commands;
using UBP.CQRS;
using UBP.Endpoints.Interfaces;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Booking.API.Endpoints;

internal sealed class RemoveSlotEndpoint : IGroupEndpoint<SlotEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{slotId:guid}", async (Guid slotId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(new RemoveAvailabilitySlotCommand(slotId), cancellationToken);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }).RequireAuthorization();
    }
}
