using OrderService.Application.Common.Exceptions;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Orders.Common;

namespace OrderService.Application.Orders.Queries.GetOrder;

public sealed class GetOrderQueryHandler(IOrderQueries queries, OrderAccessPolicy access)
{
    public async Task<OrderDto> HandleAsync(GetOrderQuery query, CancellationToken cancellationToken)
    {
        var order = await queries.GetByIdAsync(query.OrderId, cancellationToken)
            ?? throw new NotFoundException("Pedido", query.OrderId);

        access.EnsureCanAccess(order.CustomerId, query.OrderId);

        return order;
    }
}
