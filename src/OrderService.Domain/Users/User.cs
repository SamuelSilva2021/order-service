using OrderService.Domain.Common;

namespace OrderService.Domain.Users;

public sealed class User
{
    private User()
    {
        Username = string.Empty;
        PasswordHash = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Username { get; private set; }

    public string PasswordHash { get; private set; }

    public UserRole Role { get; private set; }

    public string? CustomerId { get; private set; }

    public static string NormalizeUsername(string username) => username.Trim().ToLowerInvariant();

    public static User Create(string username, string passwordHash, UserRole role, string? customerId)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new DomainException("O nome de usuário é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("O hash da senha é obrigatório.");
        }

        if (role == UserRole.Customer && string.IsNullOrWhiteSpace(customerId))
        {
            throw new DomainException("Um usuário do tipo Customer deve estar vinculado a um customerId.");
        }

        return new User
        {
            Id = Guid.NewGuid(),
            Username = NormalizeUsername(username),
            PasswordHash = passwordHash,
            Role = role,
            CustomerId = string.IsNullOrWhiteSpace(customerId) ? null : customerId.Trim()
        };
    }
}
