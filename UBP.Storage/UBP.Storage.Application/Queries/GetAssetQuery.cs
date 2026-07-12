using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;
using UBP.Storage.Abstractions;
using UBP.Storage.Application.Dtos;
using UBP.Storage.Application.Errors;
using UBP.Storage.Domain.Entities;

namespace UBP.Storage.Application.Queries;

public sealed record GetAssetQuery(Guid AssetId) : IRequest<Result<AssetDto>>;

internal sealed class GetAssetQueryHandler(IUnitOfWork unitOfWork, IAssetStorage assetStorage)
    : IRequestHandler<GetAssetQuery, Result<AssetDto>>
{
    public async Task<Result<AssetDto>> HandleAsync(GetAssetQuery request, CancellationToken cancellationToken = default)
    {
        var asset = await unitOfWork.Repository<AssetEntity>().GetSingleOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);

        return asset is null
            ? Result.Failure<AssetDto>(AssetErrors.NotFound)
            : Result.Success(AssetDto.FromEntity(asset, assetStorage.GetAccessUrl(asset.StorageKey)));
    }
}
