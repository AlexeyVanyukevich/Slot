using UBP.Storage.Domain.Entities;

namespace UBP.Storage.Application.Dtos;

public sealed record AssetDto(Guid Id, string ContentType, long SizeBytes, string AccessUrl, DateTimeOffset CreatedAt)
{
    public static AssetDto FromEntity(AssetEntity asset, string accessUrl) => new(
        asset.Id, asset.ContentType, asset.SizeBytes, accessUrl, asset.CreatedAt);
}
