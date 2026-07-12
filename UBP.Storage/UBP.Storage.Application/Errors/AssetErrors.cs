using UBP.Results;

namespace UBP.Storage.Application.Errors;

public static class AssetErrors
{
    public const string NotFoundCode = "Asset.NotFound";
    public static readonly Error NotFound = new(NotFoundCode, "The asset was not found.");

    public const string PolicyViolationCode = "Asset.PolicyViolation";
    public static Error PolicyViolation(string reason) => new(PolicyViolationCode, reason);
}
