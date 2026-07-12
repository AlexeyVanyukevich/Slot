using Microsoft.AspNetCore.Routing;

using UBP.CQRS;
using UBP.Endpoints.Interfaces;
using UBP.Storage.Application.Queries;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Storage.API.Endpoints;

internal sealed class GetAssetEndpoint : IGroupEndpoint<AssetEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(new GetAssetQuery(id), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }).AllowAnonymous();
    }
}
