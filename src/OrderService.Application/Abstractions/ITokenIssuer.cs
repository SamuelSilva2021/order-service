using OrderService.Application.Auth;
using OrderService.Application.Auth.Dtos;

namespace OrderService.Application.Abstractions;

public interface ITokenIssuer
{
    TokenDto Issue(AuthenticatedUser user);
}
