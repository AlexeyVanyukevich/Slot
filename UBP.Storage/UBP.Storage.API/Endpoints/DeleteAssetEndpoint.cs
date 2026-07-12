using Microsoft.AspNetCore.Routing;

using UBP.CQRS;
using UBP.Endpoints.Interfaces;
using UBP.Storage.Application.Commands;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Storage.API.Endpoints;

internal sealed class DeleteAssetEndpoint : IGroupEndpoint<AssetEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(new DeleteAssetCommand(id), cancellationToken);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }).RequireAuthorization();
    }
}
