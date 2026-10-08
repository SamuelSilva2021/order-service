using OrderService.Domain.Orders;

namespace OrderService.Application.Orders;

public sealed record OrderSummaryDto(
    Guid Id,
    string CustomerId,
    OrderStatus Status,
    string Currency,
    decimal Total,
    DateTimeOffset CreatedAt,
    int ItemCount);
