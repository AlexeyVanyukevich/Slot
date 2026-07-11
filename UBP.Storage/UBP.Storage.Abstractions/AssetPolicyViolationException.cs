namespace UBP.Storage.Abstractions;

public sealed class AssetPolicyViolationException : Exception
{
    public AssetPolicyViolationException(string message) : base(message)
    {
    }

    public AssetPolicyViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
