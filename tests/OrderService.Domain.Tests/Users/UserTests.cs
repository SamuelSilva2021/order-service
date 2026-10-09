using OrderService.Domain.Common;
using OrderService.Domain.Users;

namespace OrderService.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_Customer_NormalizesUsernameAndKeepsCustomerId()
    {
        var user = User.Create("  Maria ", "hash", UserRole.Customer, " customer-1 ");

        Assert.Equal("maria", user.Username);
        Assert.Equal("customer-1", user.CustomerId);
        Assert.Equal(UserRole.Customer, user.Role);
    }

    [Fact]
    public void Create_AdminWithoutCustomer_Succeeds()
    {
        var user = User.Create("admin", "hash", UserRole.Admin, null);

        Assert.Null(user.CustomerId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_CustomerWithoutCustomerId_Throws(string? customerId)
    {
        Assert.Throws<DomainException>(() => User.Create("maria", "hash", UserRole.Customer, customerId));
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("maria", "")]
    public void Create_MissingData_Throws(string username, string passwordHash)
    {
        Assert.Throws<DomainException>(() => User.Create(username, passwordHash, UserRole.Admin, null));
    }
}
