using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Auth.Commands.IssueToken;
using OrderService.Application.Auth.Common;

namespace OrderService.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("token")]
    public async Task<ActionResult<TokenDto>> IssueToken(
        [FromBody] IssueTokenCommand command,
        [FromServices] IssueTokenCommandHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(command, cancellationToken));
}
