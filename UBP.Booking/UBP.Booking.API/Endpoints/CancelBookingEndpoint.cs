using Microsoft.AspNetCore.Routing;

using UBP.Booking.Application.Commands;
using UBP.Booking.Domain.Enums;
using UBP.CQRS;
using UBP.Endpoints.Interfaces;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Booking.API.Endpoints;

internal sealed class CancelBookingEndpoint : IGroupEndpoint<BookingEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{id:guid}/cancel", async (Guid id, CancelBookingRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(new CancelBookingCommand(id, request.Reason, ChangedByType.TenantUser), cancellationToken);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }).RequireAuthorization();
    }
}

internal sealed record CancelBookingRequest(string? Reason);
