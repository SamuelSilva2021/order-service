using OrderService.Application.Common.Exceptions;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Orders.Common;

namespace OrderService.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(
    IOrderRepository orders,
    OrderStockService stock,
    OrderAccessPolicy access,
    IUnitOfWork unitOfWork)
{
    public async Task<OrderDto> HandleAsync(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException("Pedido", command.OrderId);

        access.EnsureCanAccess(order.CustomerId, command.OrderId);

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
