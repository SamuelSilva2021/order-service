using OrderService.Application.Common;
using OrderService.Application.Orders.Dtos;
using OrderService.Application.Orders.Requests;

namespace OrderService.Application.Abstractions;

public interface IOrderQueries
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<OrderSummaryDto>> SearchAsync(ListOrdersQuery query, CancellationToken cancellationToken);
}
