using OrderService.Application.Abstractions;
using OrderService.Application.Common;
using OrderService.Domain.Orders;

namespace OrderService.Application.Orders;

public sealed class GetOrderUseCase(IOrderQueries queries, OrderAccessPolicy access)
{
    public async Task<OrderDto> ExecuteAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await queries.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), orderId);

        access.EnsureCanAccess(order.CustomerId, orderId);

        return order;
    }
}
