using OrderService.Application.Abstractions;
using OrderService.Application.Common;
using OrderService.Domain.Orders;

namespace OrderService.Application.Orders;

public sealed class OrderAccessPolicy(ICurrentUser user)
{
    public void EnsureCanPlaceFor(string customerId)
    {
        if (!CanAccess(customerId))
        {
            throw new ForbiddenException("You can only place orders for your own customer id.");
        }
    }

    public void EnsureCanAccess(string customerId, Guid orderId)
    {
        if (!CanAccess(customerId))
        {
            throw new NotFoundException(nameof(Order), orderId);
        }
    }

    public string? ResolveCustomerFilter(string? requestedCustomerId)
    {
        if (user.IsAdmin)
        {
            return requestedCustomerId;
        }

        if (string.IsNullOrWhiteSpace(requestedCustomerId))
        {
            return user.CustomerId;
        }

        if (!CanAccess(requestedCustomerId))
        {
            throw new ForbiddenException("You can only list your own orders.");
        }

        return user.CustomerId;
    }

    private bool CanAccess(string customerId) =>
        user.IsAdmin || (user.CustomerId is not null && user.CustomerId == customerId.Trim());
}
