using OrderService.Application.Abstractions;
using OrderService.Application.Common;
using OrderService.Application.Orders.Dtos;
using OrderService.Application.Orders.Services;

namespace OrderService.Application.Orders.UseCases;

public sealed class CancelOrderUseCase(
    IOrderRepository orders,
    OrderStockService stock,
    OrderAccessPolicy access,
    IUnitOfWork unitOfWork)
{
    public async Task<OrderDto> ExecuteAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException("Pedido", orderId);

        access.EnsureCanAccess(order.CustomerId, orderId);

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
