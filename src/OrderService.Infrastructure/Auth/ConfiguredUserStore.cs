using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using OrderService.Application.Abstractions;
using OrderService.Application.Auth;

namespace OrderService.Infrastructure.Auth;

public sealed class ConfiguredUserStore(IOptions<ConfiguredUsersOptions> options) : IUserStore
{
    private readonly IReadOnlyList<ConfiguredUser> users = options.Value.Users;

    public AuthenticatedUser? Validate(string username, string password)
    {
        var user = users.FirstOrDefault(candidate =>
            string.Equals(candidate.Username, username, StringComparison.OrdinalIgnoreCase));

        return user is not null && PasswordMatches(user.Password, password)
            ? new AuthenticatedUser(user.Username, user.Role)
            : null;
    }

    private static bool PasswordMatches(string expected, string provided) =>
        CryptographicOperations.FixedTimeEquals(Hash(expected), Hash(provided));

    private static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
}
