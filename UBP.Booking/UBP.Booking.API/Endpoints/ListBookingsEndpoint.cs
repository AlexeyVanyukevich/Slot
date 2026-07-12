using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using UBP.Booking.Application.Queries;
using UBP.Booking.Domain.Enums;
using UBP.CQRS;
using UBP.Endpoints.Interfaces;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Booking.API.Endpoints;

internal sealed class ListBookingsEndpoint : IGroupEndpoint<BookingEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", async ([AsParameters] ListBookingsRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(
                new ListBookingsQuery(request.ResourceId, request.TenantId, request.Status, request.FromUtc, request.ToUtc, request.CustomerSearch),
                cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }).RequireAuthorization();
    }
}

internal sealed record ListBookingsRequest(Guid? ResourceId, Guid? TenantId, BookingStatus? Status, DateTime? FromUtc, DateTime? ToUtc, string? CustomerSearch);
