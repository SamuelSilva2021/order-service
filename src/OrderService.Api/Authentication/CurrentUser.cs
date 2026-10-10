using System.Security.Claims;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Common.Security;
using OrderService.Infrastructure.Auth;

namespace OrderService.Api.Authentication;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAdmin => Principal?.FindFirst(JwtClaimNames.Role)?.Value == Roles.Admin;

    public string? CustomerId => Principal?.FindFirst(JwtClaimNames.CustomerId)?.Value;
}
