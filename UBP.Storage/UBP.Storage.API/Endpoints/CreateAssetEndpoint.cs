using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using UBP.CQRS;
using UBP.Endpoints.Interfaces;
using UBP.Storage.Application.Commands;

using static Microsoft.AspNetCore.Http.Results;

namespace UBP.Storage.API.Endpoints;

internal sealed class CreateAssetEndpoint : IGroupEndpoint<AssetEndpointGroupV1>
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", async (IFormFile file, ISender sender, CancellationToken cancellationToken) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await sender.SendAsync(new CreateAssetCommand(file.FileName, file.ContentType, stream), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }).RequireAuthorization();
    }
}
