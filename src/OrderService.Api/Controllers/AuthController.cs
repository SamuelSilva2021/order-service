using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Auth;

namespace OrderService.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("token")]
    public async Task<ActionResult<TokenDto>> IssueToken(
        [FromBody] IssueTokenCommand command,
        [FromServices] IssueTokenUseCase useCase,
        CancellationToken cancellationToken) =>
        Ok(await useCase.ExecuteAsync(command, cancellationToken));
}
