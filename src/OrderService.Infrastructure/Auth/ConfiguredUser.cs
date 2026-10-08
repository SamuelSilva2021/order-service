namespace OrderService.Infrastructure.Auth;

public sealed class ConfiguredUser
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}
