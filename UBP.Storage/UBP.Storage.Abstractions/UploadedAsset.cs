namespace UBP.Storage.Abstractions;

public sealed record UploadedAsset(string StorageKey, string ContentType, long SizeBytes);
