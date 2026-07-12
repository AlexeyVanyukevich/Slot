using Microsoft.AspNetCore.Routing;

using UBP.Booking.Application.Queries;
using UBP.CQRS;
using UBP.Endpoints.Interfaces;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Booking.API.Endpoints;

internal sealed class GetAvailableSlotsEndpoint : IGroupEndpoint<SlotEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", async (Guid resourceId, DateTime from, DateTime to, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(new GetAvailableSlotsQuery(resourceId, from, to), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }).AllowAnonymous();
    }
}
