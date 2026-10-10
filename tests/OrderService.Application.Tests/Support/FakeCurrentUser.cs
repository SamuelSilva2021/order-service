using OrderService.Application.Common.Interfaces;

namespace OrderService.Application.Tests.Support;

public sealed class FakeCurrentUser(bool isAdmin, string? customerId) : ICurrentUser
{
    public static FakeCurrentUser Admin => new(true, null);

    public static FakeCurrentUser Customer(string customerId = "customer-1") => new(false, customerId);

    public bool IsAdmin { get; } = isAdmin;

    public string? CustomerId { get; } = customerId;
}
