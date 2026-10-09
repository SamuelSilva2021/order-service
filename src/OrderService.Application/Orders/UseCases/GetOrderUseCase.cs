using OrderService.Application.Abstractions;
using OrderService.Application.Common;
using OrderService.Application.Orders.Dtos;
using OrderService.Application.Orders.Services;

namespace OrderService.Application.Orders.UseCases;

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
