using OrderService.Domain.Orders;

namespace OrderService.Application.Orders.Requests;

public sealed record ListOrdersQuery(
    string? CustomerId = null,
    OrderStatus? Status = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Page = 1,
    int PageSize = 20);
