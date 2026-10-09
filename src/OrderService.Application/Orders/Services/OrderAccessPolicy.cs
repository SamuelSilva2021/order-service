using OrderService.Application.Abstractions;
using OrderService.Application.Common;

namespace OrderService.Application.Orders.Services;

public sealed class OrderAccessPolicy(ICurrentUser user)
{
    public void EnsureCanPlaceFor(string customerId)
    {
        if (!CanAccess(customerId))
            throw new ForbiddenException("Você só pode criar pedidos para o seu próprio cliente.");
    }

    public void EnsureCanAccess(string customerId, Guid orderId)
    {
        if (!CanAccess(customerId))
            throw new NotFoundException("Pedido", orderId);
    }

    public string? ResolveCustomerFilter(string? requestedCustomerId)
    {
        if (user.IsAdmin)
            return requestedCustomerId;

        if (string.IsNullOrWhiteSpace(requestedCustomerId))
            return user.CustomerId;

        if (!CanAccess(requestedCustomerId))
            throw new ForbiddenException("Você só pode listar os seus próprios pedidos.");

        return user.CustomerId;
    }

    private bool CanAccess(string customerId) =>
        user.IsAdmin || (user.CustomerId is not null && user.CustomerId == customerId.Trim());
}
