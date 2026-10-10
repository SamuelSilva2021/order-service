using OrderService.Domain.Users;

namespace OrderService.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken);
}
