using Microsoft.AspNetCore.Identity;

using UBP.CQRS;
using UBP.IAM.Application.Errors;
using UBP.IAM.Domain.Entities;
using UBP.IAM.Persistence.Interfaces;
using UBP.Results;

namespace UBP.IAM.Application.Commands;

public record RegisterUserCommand(string Email, string Password) : IRequest<Result>;

internal sealed class RegisterUserCommandHandler(IUserRepository userRepository) : IRequestHandler<RegisterUserCommand, Result>
{
    public async Task<Result> HandleAsync(RegisterUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email
        };

        IdentityResult result = await userRepository.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            bool isDuplicate = result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName");
            return isDuplicate
                ? Result.Failure(UserErrors.AlreadyExists)
                : Result.Failure(UserErrors.RegistrationFailed(result.Errors));
        }

        return Result.Success();
    }
}
