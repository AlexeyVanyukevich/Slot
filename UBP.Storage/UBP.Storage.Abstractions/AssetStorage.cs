namespace UBP.Storage.Abstractions;

public abstract class AssetStorage(AssetStorageOptions options) : IAssetStorage
{
    public async Task<UploadedAsset> UploadAsync(UploadAssetRequest request, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(request.StorageKey);

        if (!options.AllowedExtensions.Contains(extension))
            throw new AssetPolicyViolationException($"File extension '{extension}' is not allowed.");

        if (request.Content.Length > options.MaxSizeBytes)
            throw new AssetPolicyViolationException($"File size exceeds the {options.MaxSizeBytes}-byte limit.");

        var sizeBytes = await WriteAsync(request.StorageKey, request.Content, cancellationToken);

        return new UploadedAsset(request.StorageKey, request.ContentType, sizeBytes);
    }

    public abstract string GetAccessUrl(string storageKey);
    public abstract Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);

    protected abstract Task<long> WriteAsync(string storageKey, Stream content, CancellationToken cancellationToken);
}
