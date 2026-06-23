using System.Security.Claims;

using UBP.CQRS;
using UBP.IAM.Application.Errors;
using UBP.IAM.Persistence.Interfaces;
using UBP.IAM.Domain.Entities;
using UBP.Results;


namespace UBP.IAM.Application.Commands;

public record ExchangeTokenCommand(string UserId, IEnumerable<Claim> ExistingClaims) : IRequest<Result<ClaimsPrincipal>>;

internal sealed class ExchangeTokenCommandHandler(
    IUserRepository userRepository,
    ISignInRepository signInRepository,
    ISender sender) : IRequestHandler<ExchangeTokenCommand, Result<ClaimsPrincipal>>
{
    public async Task<Result<ClaimsPrincipal>> HandleAsync(ExchangeTokenCommand request, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userRepository.FindByIdAsync(request.UserId);

        if (user is null)
            return Result.Failure<ClaimsPrincipal>(TokenErrors.Invalid);

        if (!await signInRepository.CanSignInAsync(user))
            return Result.Failure<ClaimsPrincipal>(UserErrors.SignInNotAllowed);

        ClaimsIdentity identity = await sender.SendAsync(new SetClaimsCommand(user, request.ExistingClaims), cancellationToken);

        return Result.Success(new ClaimsPrincipal(identity));
    }
}
