using OrderService.Application.Auth;

namespace OrderService.Application.Abstractions;

public interface ITokenIssuer
{
    TokenDto Issue(AuthenticatedUser user);
}
