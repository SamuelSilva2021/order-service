using OrderService.Domain.Users;

namespace OrderService.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken);
}
