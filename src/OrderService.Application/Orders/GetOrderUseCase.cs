using OrderService.Application.Abstractions;
using OrderService.Application.Common;

namespace OrderService.Application.Orders;

public sealed class GetOrderUseCase(IOrderQueries queries, OrderAccessPolicy access)
{
    public async Task<OrderDto> ExecuteAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await queries.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException("Pedido", orderId);

        access.EnsureCanAccess(order.CustomerId, orderId);

        return order;
    }
}
