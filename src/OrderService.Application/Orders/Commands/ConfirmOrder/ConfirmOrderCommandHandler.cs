using OrderService.Application.Common.Exceptions;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Orders.Common;

namespace OrderService.Application.Orders.Commands.ConfirmOrder;

public sealed class ConfirmOrderCommandHandler(
    IOrderRepository orders,
    OrderStockService stock,
    OrderAccessPolicy access,
    IUnitOfWork unitOfWork)
{
    public async Task<OrderDto> HandleAsync(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException("Pedido", command.OrderId);

        access.EnsureCanAccess(order.CustomerId, command.OrderId);

        if (order.Confirm())
        {
            await stock.ReserveAsync(order, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return OrderDto.From(order);
    }
}
