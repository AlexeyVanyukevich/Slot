using Microsoft.AspNetCore.Mvc;

using UBP.CQRS;
using UBP.IAM.Application.Commands;
using UBP.Results;

namespace UBP.IAM.Host.Controllers;

internal sealed class AccountController(ISender sender) : Controller
{
    [HttpPost("~/auth/register")]
    [Produces("application/json")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        Result result = await sender.SendAsync(
            new RegisterUserCommand(request.Email, request.Password));

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok();
    }
}

internal sealed record RegisterRequest(string Email, string Password);
