using OrderService.Domain.Orders;

namespace OrderService.Application.Orders.Dtos;

public sealed record OrderDto(
    Guid Id,
    string CustomerId,
    OrderStatus Status,
    string Currency,
    decimal Total,
    DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemDto> Items)
{
    public static OrderDto From(Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.Status,
            order.Currency.Code,
            order.Total,
            order.CreatedAt,
            order.Items.Select(OrderItemDto.From).ToList());
}
