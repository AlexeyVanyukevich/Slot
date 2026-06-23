using System.Security.Claims;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;

using UBP.CQRS;
using UBP.IAM.Application.Commands;
using UBP.IAM.Application.Queries;
using UBP.Results;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace UBP.IAM.Host.Controllers;

internal sealed class AuthorizationController(ISender sender) : Controller
{
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();
        if (request is null)
            return ServerError("The OpenIddict server request cannot be retrieved.");

        AuthenticateResult result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? [.. Request.Form] : [.. Request.Query])
                });
        }

        string? userId = result.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return AccessDenied("The user details cannot be retrieved.");

        Result<ClaimsPrincipal> authorizeResult = await sender.SendAsync(
            new AuthorizeUserCommand(userId, request.GetScopes()));

        if (!authorizeResult.IsSuccess)
            return InvalidGrant("The user is not allowed to sign in.");

        return SignIn(authorizeResult.Value, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();
        if (request is null)
            return ServerError("The OpenIddict server request cannot be retrieved.");

        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            return UnsupportedGrantType("The specified grant type is not supported.");

        AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        string? userId = result.Principal!.GetClaim(Claims.Subject);
        if (userId is null)
            return InvalidGrant("The token is no longer valid.");

        Result<ClaimsPrincipal> exchangeResult = await sender.SendAsync(
            new ExchangeTokenCommand(userId, result.Principal!.Claims));

        if (!exchangeResult.IsSuccess)
            return InvalidGrant("The user is no longer allowed to sign in.");

        return SignIn(exchangeResult.Value, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public async Task<IActionResult> UserInfo()
    {
        string? userId = User.GetClaim(Claims.Subject);
        if (userId is null)
        {
            return InvalidToken("The user profile cannot be retrieved.");
        }

        Result<Dictionary<string, object>> claims = await sender.SendAsync(
            new GetUserInfoQuery(userId, User.GetScopes()));

        return Ok(claims);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        await sender.SendAsync(new LogoutCommand());

        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties { RedirectUri = "/" });
    }

    private ForbidResult ServerError(string description) =>
        OpenIddictForbid(Errors.ServerError, description);

    private ForbidResult AccessDenied(string description) =>
        OpenIddictForbid(Errors.AccessDenied, description);

    private ForbidResult InvalidGrant(string description) =>
        OpenIddictForbid(Errors.InvalidGrant, description);

    private ForbidResult UnsupportedGrantType(string description) =>
        OpenIddictForbid(Errors.UnsupportedGrantType, description);

    private ForbidResult InvalidToken(string description) =>
        OpenIddictForbid(Errors.InvalidToken, description);

    private ForbidResult OpenIddictForbid(string error, string description) =>
        Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }));
}
