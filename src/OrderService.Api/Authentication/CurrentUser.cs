using System.Security.Claims;
using OrderService.Application.Abstractions;
using OrderService.Application.Auth;
using OrderService.Infrastructure.Auth;

namespace OrderService.Api.Authentication;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAdmin => Principal?.FindFirst(JwtClaimNames.Role)?.Value == Roles.Admin;

    public string? CustomerId => Principal?.FindFirst(JwtClaimNames.CustomerId)?.Value;
}
