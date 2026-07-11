namespace UBP.Storage.Abstractions;

public interface IAssetStorage
{
    Task<UploadedAsset> UploadAsync(UploadAssetRequest request, CancellationToken cancellationToken = default);
    string GetAccessUrl(string storageKey);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
