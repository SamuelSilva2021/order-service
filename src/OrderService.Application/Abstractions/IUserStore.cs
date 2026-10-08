using OrderService.Application.Auth;

namespace OrderService.Application.Abstractions;

public interface IUserStore
{
    AuthenticatedUser? Validate(string username, string password);
}
