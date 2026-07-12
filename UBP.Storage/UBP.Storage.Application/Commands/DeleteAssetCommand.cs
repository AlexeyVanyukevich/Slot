using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;
using UBP.Storage.Abstractions;
using UBP.Storage.Application.Errors;
using UBP.Storage.Domain.Entities;

namespace UBP.Storage.Application.Commands;

public sealed record DeleteAssetCommand(Guid AssetId) : IRequest<Result>;

internal sealed class DeleteAssetCommandHandler(IUnitOfWork unitOfWork, IAssetStorage assetStorage)
    : IRequestHandler<DeleteAssetCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteAssetCommand request, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<AssetEntity>();
        var asset = await repository.GetSingleOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);
        if (asset is null)
            return Result.Failure(AssetErrors.NotFound);

        await assetStorage.DeleteAsync(asset.StorageKey, cancellationToken);

        repository.Delete(asset);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
