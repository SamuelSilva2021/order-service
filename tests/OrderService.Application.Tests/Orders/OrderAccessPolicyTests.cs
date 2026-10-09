using OrderService.Application.Common;
using OrderService.Application.Orders;
using OrderService.Application.Tests.Support;

namespace OrderService.Application.Tests.Orders;

public class OrderAccessPolicyTests
{
    private static OrderAccessPolicy PolicyFor(FakeCurrentUser user) => new(user);

    [Fact]
    public void EnsureCanPlaceFor_Admin_AllowsAnyCustomer()
    {
        PolicyFor(FakeCurrentUser.Admin).EnsureCanPlaceFor("anyone");
    }

    [Fact]
    public void EnsureCanPlaceFor_CustomerForSelf_Allows()
    {
        PolicyFor(FakeCurrentUser.Customer("customer-1")).EnsureCanPlaceFor("customer-1");
    }

    [Fact]
    public void EnsureCanPlaceFor_CustomerForSomeoneElse_ThrowsForbidden()
    {
        var policy = PolicyFor(FakeCurrentUser.Customer("customer-1"));

        Assert.Throws<ForbiddenException>(() => policy.EnsureCanPlaceFor("customer-2"));
    }

    [Fact]
    public void EnsureCanAccess_CustomerOnOtherOrder_ThrowsNotFound()
    {
        var policy = PolicyFor(FakeCurrentUser.Customer("customer-1"));

        Assert.Throws<NotFoundException>(() => policy.EnsureCanAccess("customer-2", Guid.NewGuid()));
    }

    [Fact]
    public void EnsureCanAccess_CustomerWithoutLinkedId_ThrowsNotFound()
    {
        var policy = PolicyFor(new FakeCurrentUser(false, null));

        Assert.Throws<NotFoundException>(() => policy.EnsureCanAccess("customer-1", Guid.NewGuid()));
    }

    [Fact]
    public void ResolveCustomerFilter_Admin_KeepsRequestedValue()
    {
        var policy = PolicyFor(FakeCurrentUser.Admin);

        Assert.Equal("customer-9", policy.ResolveCustomerFilter("customer-9"));
        Assert.Null(policy.ResolveCustomerFilter(null));
    }

    [Fact]
    public void ResolveCustomerFilter_CustomerWithoutRequest_UsesOwnId()
    {
        var policy = PolicyFor(FakeCurrentUser.Customer("customer-1"));

        Assert.Equal("customer-1", policy.ResolveCustomerFilter(null));
    }

    [Fact]
    public void ResolveCustomerFilter_CustomerRequestingOthers_ThrowsForbidden()
    {
        var policy = PolicyFor(FakeCurrentUser.Customer("customer-1"));

        Assert.Throws<ForbiddenException>(() => policy.ResolveCustomerFilter("customer-2"));
    }
}
