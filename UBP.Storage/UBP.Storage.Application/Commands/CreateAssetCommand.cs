using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;
using UBP.Storage.Abstractions;
using UBP.Storage.Application.Dtos;
using UBP.Storage.Application.Errors;
using UBP.Storage.Domain.Entities;

namespace UBP.Storage.Application.Commands;

public sealed record CreateAssetCommand(string FileName, string ContentType, Stream Content) : IRequest<Result<AssetDto>>;

internal sealed class CreateAssetCommandHandler(IUnitOfWork unitOfWork, IAssetStorage assetStorage)
    : IRequestHandler<CreateAssetCommand, Result<AssetDto>>
{
    public async Task<Result<AssetDto>> HandleAsync(CreateAssetCommand request, CancellationToken cancellationToken = default)
    {
        var assetId = Guid.CreateVersion7();
        var storageKey = $"{assetId}{Path.GetExtension(request.FileName)}";

        UploadedAsset uploaded;
        try
        {
            uploaded = await assetStorage.UploadAsync(
                new UploadAssetRequest(storageKey, request.ContentType, request.Content),
                cancellationToken);
        }
        catch (AssetPolicyViolationException ex)
        {
            return Result.Failure<AssetDto>(AssetErrors.PolicyViolation(ex.Message));
        }

        var asset = new AssetEntity
        {
            Id = assetId,
            StorageKey = uploaded.StorageKey,
            ContentType = uploaded.ContentType,
            SizeBytes = uploaded.SizeBytes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        unitOfWork.Repository<AssetEntity>().Add(asset);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(AssetDto.FromEntity(asset, assetStorage.GetAccessUrl(asset.StorageKey)));
    }
}
