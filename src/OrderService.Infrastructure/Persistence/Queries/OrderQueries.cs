using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Common.Models;
using OrderService.Application.Orders.Common;
using OrderService.Application.Orders.Queries.ListOrders;
using OrderService.Domain.Orders;

namespace OrderService.Infrastructure.Persistence.Queries;

public sealed class OrderQueries(AppDbContext context) : IOrderQueries
{
    public Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Orders
            .AsNoTracking()
            .Where(order => order.Id == id)
            .Select(order => new OrderDto(
                order.Id,
                order.CustomerId,
                order.Status,
                order.Currency.Code,
                order.Total,
                order.CreatedAt,
                order.Items
                    .Select(item => new OrderItemDto(item.ProductId, item.UnitPrice, item.Quantity, item.UnitPrice * item.Quantity))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<OrderSummaryDto>> SearchAsync(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = Filter(context.Orders.AsNoTracking(), query);

        var totalCount = await orders.CountAsync(cancellationToken);

        var items = await orders
            .OrderByDescending(order => order.CreatedAt)
            .ThenBy(order => order.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(order => new OrderSummaryDto(
                order.Id,
                order.CustomerId,
                order.Status,
                order.Currency.Code,
                order.Total,
                order.CreatedAt,
                order.Items.Count))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderSummaryDto>(items, query.Page, query.PageSize, totalCount);
    }

    private static IQueryable<Order> Filter(IQueryable<Order> orders, ListOrdersQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.CustomerId))
            orders = orders.Where(order => order.CustomerId == query.CustomerId);

        if (query.Status is { } status)
            orders = orders.Where(order => order.Status == status);

        if (query.From is { } from)
        {
            var fromUtc = from.ToUniversalTime();
            orders = orders.Where(order => order.CreatedAt >= fromUtc);
        }

        if (query.To is { } to)
        {
            var toUtc = to.ToUniversalTime();
            orders = orders.Where(order => order.CreatedAt <= toUtc);
        }

        return orders;
    }
}
