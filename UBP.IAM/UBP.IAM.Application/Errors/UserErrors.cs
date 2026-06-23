using Microsoft.AspNetCore.Identity;

using UBP.Results;

namespace UBP.IAM.Application.Errors;

public static class UserErrors
{
    public const string NotFoundCode = "User.NotFound";
    public static readonly Error NotFound = new(NotFoundCode, "The user was not found.");

    public const string SignInNotAllowedCode = "User.SignInNotAllowed";
    public static readonly Error SignInNotAllowed = new(SignInNotAllowedCode, "The user is no longer allowed to sign in.");

    public const string AlreadyExistsCode = "User.AlreadyExists";
    public static readonly Error AlreadyExists = new(AlreadyExistsCode, "A user with this email already exists.");

    public static Error RegistrationFailed(IEnumerable<IdentityError> errors) =>
        new("User.RegistrationFailed", string.Join(" ", errors.Select(e => e.Description)));
}
