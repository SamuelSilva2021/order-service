using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Users;

namespace OrderService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var normalized = User.NormalizeUsername(username);

        return context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == normalized, cancellationToken);
    }
}
