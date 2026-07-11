namespace UBP.Storage.Abstractions;

public sealed record UploadAssetRequest(string StorageKey, string ContentType, Stream Content);
