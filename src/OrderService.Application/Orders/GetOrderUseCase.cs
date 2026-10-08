using OrderService.Application.Abstractions;
using OrderService.Application.Common;
using OrderService.Domain.Orders;

namespace OrderService.Application.Orders;

public sealed class GetOrderUseCase(IOrderQueries queries)
{
    public async Task<OrderDto> ExecuteAsync(Guid orderId, CancellationToken cancellationToken) =>
        await queries.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), orderId);
}
