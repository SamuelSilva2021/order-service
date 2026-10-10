using OrderService.Application.Auth.Common;

namespace OrderService.Application.Common.Interfaces;

public interface ITokenIssuer
{
    TokenDto Issue(AuthenticatedUser user);
}
