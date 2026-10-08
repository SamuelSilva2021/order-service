using OrderService.Domain.Common;

namespace OrderService.Domain.Orders;

public sealed class InvalidOrderTransitionException(Guid orderId, OrderStatus from, OrderStatus to)
    : DomainException($"Order {orderId} cannot transition from {from} to {to}.");
