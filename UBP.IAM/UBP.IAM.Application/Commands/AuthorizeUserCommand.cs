using OpenIddict.Abstractions;

using System.Security.Claims;

using UBP.CQRS;
using UBP.IAM.Application.Errors;
using UBP.IAM.Persistence.Interfaces;
using UBP.IAM.Domain.Entities;
using UBP.Results;


namespace UBP.IAM.Application.Commands;

public record AuthorizeUserCommand(string UserId, IEnumerable<string> RequestedScopes) : IRequest<Result<ClaimsPrincipal>>;

internal sealed class AuthorizeUserCommandHandler(
    IUserRepository userService,
    IOpenIddictScopeManager scopeManager,
    ISender sender) : IRequestHandler<AuthorizeUserCommand, Result<ClaimsPrincipal>>
{
    public async Task<Result<ClaimsPrincipal>> HandleAsync(AuthorizeUserCommand request, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userService.FindByIdAsync(request.UserId);

        if (user is null)
            return Result.Failure<ClaimsPrincipal>(UserErrors.NotFound);

        ClaimsIdentity identity = await sender.SendAsync(new SetClaimsCommand(user), cancellationToken);

        identity.SetScopes(request.RequestedScopes);

        var resources = new List<string>();
        await foreach (var resource in scopeManager.ListResourcesAsync(identity.GetScopes(), cancellationToken))
            resources.Add(resource);

        identity.SetResources(resources);

        return Result.Success(new ClaimsPrincipal(identity));
    }
}
