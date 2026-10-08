using OrderService.Application.Common;
using OrderService.Application.Orders;

namespace OrderService.Application.Abstractions;

public interface IOrderQueries
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<OrderSummaryDto>> SearchAsync(ListOrdersQuery query, CancellationToken cancellationToken);
}
