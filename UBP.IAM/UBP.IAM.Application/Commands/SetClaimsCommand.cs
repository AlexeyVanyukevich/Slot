using OpenIddict.Abstractions;
using System.Security.Claims;

using UBP.CQRS;
using UBP.IAM.Domain.Entities;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace UBP.IAM.Application.Commands;

internal sealed record SetClaimsCommand(ApplicationUser User, IEnumerable<Claim>? Claims = null) : IRequest<ClaimsIdentity>;

internal sealed class SetClaimsCommandHandler() : IRequestHandler<SetClaimsCommand, ClaimsIdentity>
{
    const string AuthenticationType = "ubp-aim";
    public async Task<ClaimsIdentity> HandleAsync(SetClaimsCommand request, CancellationToken cancellationToken = default)
    {
        ClaimsIdentity identity = new(
            request.Claims,
            AuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, request.User.Id)
                .SetClaim(Claims.Email, request.User.Email)
                .SetClaim(Claims.Name, request.User.UserName);

        identity.SetDestinations(GetDestinations);

        return identity;
    }

    static IEnumerable<string> GetDestinations(Claim claim)
    {
        if (claim.Type is Claims.Name or Claims.Subject)
            return [Destinations.AccessToken, Destinations.IdentityToken];

        if (claim.Type is Claims.Email && claim.Subject?.HasScope(Scopes.Email) is true)
            return [Destinations.AccessToken, Destinations.IdentityToken];

        return [Destinations.AccessToken];
    }
}