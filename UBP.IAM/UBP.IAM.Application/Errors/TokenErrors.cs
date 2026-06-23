using UBP.Results;

namespace UBP.IAM.Application.Errors;

public static class TokenErrors
{
    public const string InvalidCode = "Token.Invalid";
    public static readonly Error Invalid = new(InvalidCode, "The token is no longer valid.");
}
