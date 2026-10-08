namespace OrderService.Infrastructure.Auth;

public sealed class ConfiguredUsersOptions
{
    public const string SectionName = "Auth";

    public List<ConfiguredUser> Users { get; init; } = [];
}
