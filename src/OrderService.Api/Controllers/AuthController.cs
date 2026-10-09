using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Auth.Dtos;
using OrderService.Application.Auth.Requests;
using OrderService.Application.Auth.UseCases;

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
