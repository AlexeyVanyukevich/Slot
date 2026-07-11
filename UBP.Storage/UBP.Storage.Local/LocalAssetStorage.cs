using UBP.Storage.Abstractions;

namespace UBP.Storage.Local;

internal sealed class LocalAssetStorage(AssetStorageOptions assetStorageOptions, LocalAssetStorageOptions localOptions)
    : AssetStorage(assetStorageOptions)
{
    protected override async Task<long> WriteAsync(string storageKey, Stream content, CancellationToken cancellationToken)
    {
        var path = ToFilePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var fileStream = File.Create(path);
        await content.CopyToAsync(fileStream, cancellationToken);
        return fileStream.Length;
    }

    public override string GetAccessUrl(string storageKey) => $"{localOptions.PublicBaseUrl.TrimEnd('/')}/{storageKey}";

    public override Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = ToFilePath(storageKey);
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    private string ToFilePath(string storageKey) =>
        Path.Combine(localOptions.RootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
}
