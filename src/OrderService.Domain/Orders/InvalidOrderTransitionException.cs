using OrderService.Domain.Common;

namespace OrderService.Domain.Orders;

public sealed class InvalidOrderTransitionException(Guid orderId, OrderStatus from, OrderStatus to)
    : DomainException($"O pedido {orderId} não pode passar de {from.ToDisplayName()} para {to.ToDisplayName()}.");
