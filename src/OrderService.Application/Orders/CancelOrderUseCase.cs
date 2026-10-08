using OrderService.Application.Abstractions;
using OrderService.Application.Common;
using OrderService.Domain.Orders;

namespace OrderService.Application.Orders;

public sealed class CancelOrderUseCase(IOrderRepository orders, OrderStockService stock, IUnitOfWork unitOfWork)
{
    public async Task<OrderDto> ExecuteAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), orderId);

        var releasesStock = order.HoldsStock;

        if (order.Cancel())
        {
            if (releasesStock)
            {
                await stock.ReleaseAsync(order, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return OrderDto.From(order);
    }
}
