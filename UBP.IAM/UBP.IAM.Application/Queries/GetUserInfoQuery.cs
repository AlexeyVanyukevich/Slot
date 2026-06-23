using UBP.CQRS;
using UBP.IAM.Application.Errors;
using UBP.IAM.Domain.Entities;
using UBP.IAM.Persistence.Interfaces;
using UBP.Results;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace UBP.IAM.Application.Queries;

public record GetUserInfoQuery(string UserId, IEnumerable<string> Scopes) : IRequest<Result<Dictionary<string, object>>>;

internal sealed class GetUserInfoQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserInfoQuery, Result<Dictionary<string, object>>>
{
    public async Task<Result<Dictionary<string, object>>> HandleAsync(GetUserInfoQuery request, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userRepository.FindByIdAsync(request.UserId);

        if (user is null)
            return Result.Failure<Dictionary<string, object>>(UserErrors.NotFound);

        Dictionary<string, object> claims = new(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Id
        };

        if (request.Scopes.Contains(Scopes.Email))
        {
            claims[Claims.Email] = user.Email ?? string.Empty;
            claims[Claims.EmailVerified] = await userRepository.IsEmailConfirmedAsync(user);
        }

        if (request.Scopes.Contains(Scopes.Profile))
        {
            claims[Claims.Name] = user.UserName ?? string.Empty;
        }

        return Result.Success(claims);
    }
}
