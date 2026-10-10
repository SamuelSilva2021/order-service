using OrderService.Application.Common.Models;
using OrderService.Application.Orders.Common;
using OrderService.Application.Orders.Queries.ListOrders;

namespace OrderService.Application.Common.Interfaces;

public interface IOrderQueries
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<OrderSummaryDto>> SearchAsync(ListOrdersQuery query, CancellationToken cancellationToken);
}
