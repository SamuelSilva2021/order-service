using OrderService.Domain.Users;

namespace OrderService.Infrastructure.Persistence.Configurations;

public static class UserSeed
{
    public static object[] All() =>
    [
        Row(
            "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            "admin",
            "v1.600000.v/1H6x41FXYgiPv5Z5WXWQ==.abEgtNOuCopuMywr7rZs8javnZMEYJFhmLW7QGkiiV4=",
            UserRole.Admin,
            null),
        Row(
            "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
            "customer",
            "v1.600000.Wth+s2v7HS1WrnlrNZN8Rg==.zxUtdxcycvnbOfhqX9BGTeoav9Q4rX7DyrCC2w8wL0M=",
            UserRole.Customer,
            "customer-001"),
        Row(
            "cccccccc-cccc-cccc-cccc-cccccccccccc",
            "customer2",
            "v1.600000.nmEmoJon3mBxxOuPr8boFg==.6bAC2WWeOxYuuQcnBasJkWrYkq1yW+UrwWBpW0z0+Ko=",
            UserRole.Customer,
            "customer-002")
    ];

    private static object Row(string id, string username, string passwordHash, UserRole role, string? customerId) =>
        new { Id = Guid.Parse(id), Username = username, PasswordHash = passwordHash, Role = role, CustomerId = customerId };
}
