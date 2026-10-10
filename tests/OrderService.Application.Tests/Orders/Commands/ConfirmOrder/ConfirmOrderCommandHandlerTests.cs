using NSubstitute;
using OrderService.Application.Common.Exceptions;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Orders.Commands.ConfirmOrder;
using OrderService.Application.Orders.Common;
using OrderService.Application.Tests.Support;
using OrderService.Domain.Orders;
using OrderService.Domain.Products;

namespace OrderService.Application.Tests.Orders.Commands.ConfirmOrder;

public class ConfirmOrderCommandHandlerTests
{
    private readonly IOrderRepository orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ConfirmOrderCommandHandler handler;

    public ConfirmOrderCommandHandlerTests()
    {
        handler = CreateHandler(FakeCurrentUser.Admin);
    }

    private ConfirmOrderCommandHandler CreateHandler(ICurrentUser user) =>
        new(orders, new OrderStockService(products), new OrderAccessPolicy(user), unitOfWork);

    private void Stored(Order order, params Product[] catalog)
    {
        orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        products.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(catalog);
    }

    [Fact]
    public async Task Execute_PlacedOrder_ConfirmsAndReservesStock()
    {
        var keyboard = Samples.Product(availableQuantity: 10);
        var order = Samples.PlacedOrder((keyboard, 4));
        Stored(order, keyboard);

        var result = await handler.HandleAsync(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(OrderStatus.Confirmed, result.Status);
        Assert.Equal(6, keyboard.AvailableQuantity);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_CalledTwice_ReservesStockOnlyOnce()
    {
        var keyboard = Samples.Product(availableQuantity: 10);
        var order = Samples.PlacedOrder((keyboard, 4));
        Stored(order, keyboard);

        var first = await handler.HandleAsync(new ConfirmOrderCommand(order.Id), CancellationToken.None);
        var second = await handler.HandleAsync(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Total, second.Total);
        Assert.Equal(6, keyboard.AvailableQuantity);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_StockConsumedMeanwhile_ThrowsAndDoesNotSave()
    {
        var keyboard = Samples.Product(availableQuantity: 4);
        var order = Samples.PlacedOrder((keyboard, 4));
        keyboard.Reserve(3);
        Stored(order, keyboard);

        await Assert.ThrowsAsync<InsufficientStockException>(() => handler.HandleAsync(new ConfirmOrderCommand(order.Id), CancellationToken.None));
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_CanceledOrder_ThrowsInvalidTransition()
    {
        var keyboard = Samples.Product();
        var order = Samples.PlacedOrder((keyboard, 1));
        order.Cancel();
        Stored(order, keyboard);

        await Assert.ThrowsAsync<InvalidOrderTransitionException>(() => handler.HandleAsync(new ConfirmOrderCommand(order.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Execute_UnknownOrder_ThrowsNotFound()
    {
        orders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new ConfirmOrderCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
