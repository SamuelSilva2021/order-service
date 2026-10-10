using OrderService.Domain.Orders;

namespace OrderService.Application.Orders.Queries.ListOrders;

public sealed record OrderSummaryDto(
    Guid Id,
    string CustomerId,
    OrderStatus Status,
    string Currency,
    decimal Total,
    DateTimeOffset CreatedAt,
    int ItemCount);
