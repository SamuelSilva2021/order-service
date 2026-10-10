using OrderService.Domain.Orders;

namespace OrderService.Application.Orders.Common;

public sealed record OrderItemDto(Guid ProductId, decimal UnitPrice, int Quantity, decimal Subtotal)
{
    public static OrderItemDto From(OrderItem item) =>
        new(item.ProductId, item.UnitPrice, item.Quantity, item.Subtotal);
}
