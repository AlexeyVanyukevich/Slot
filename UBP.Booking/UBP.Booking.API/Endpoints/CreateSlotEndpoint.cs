using Microsoft.AspNetCore.Routing;

using UBP.Booking.Application.Commands;
using UBP.CQRS;
using UBP.Endpoints.Interfaces;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Booking.API.Endpoints;

internal sealed class CreateSlotEndpoint : IGroupEndpoint<SlotEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", async (CreateSlotRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(
                new CreateAvailabilitySlotCommand(request.TenantId, request.ResourceId, request.StartAt, request.EndAt, request.Capacity),
                cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }).RequireAuthorization();
    }
}

internal sealed record CreateSlotRequest(Guid TenantId, Guid ResourceId, DateTime StartAt, DateTime EndAt, int Capacity = 1);
