using NSubstitute;
using OrderService.Application.Common.Exceptions;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Orders.Commands.CancelOrder;
using OrderService.Application.Orders.Common;
using OrderService.Application.Tests.Support;
using OrderService.Domain.Orders;
using OrderService.Domain.Products;

namespace OrderService.Application.Tests.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandlerTests
{
    private readonly IOrderRepository orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CancelOrderCommandHandler handler;

    public CancelOrderCommandHandlerTests()
    {
        handler = CreateHandler(FakeCurrentUser.Admin);
    }

    private CancelOrderCommandHandler CreateHandler(ICurrentUser user) =>
        new(orders, new OrderStockService(products), new OrderAccessPolicy(user), unitOfWork);

    private void Stored(Order order, params Product[] catalog)
    {
        orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        products.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(catalog);
    }

    [Fact]
    public async Task Execute_PlacedOrder_CancelsWithoutTouchingStock()
    {
        var keyboard = Samples.Product(availableQuantity: 10);
        var order = Samples.PlacedOrder((keyboard, 4));
        Stored(order, keyboard);

        var result = await handler.HandleAsync(new CancelOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(OrderStatus.Canceled, result.Status);
        Assert.Equal(10, keyboard.AvailableQuantity);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ConfirmedOrder_CancelsAndReleasesStock()
    {
        var keyboard = Samples.Product(availableQuantity: 10);
        keyboard.Reserve(4);
        var order = Samples.ConfirmedOrder((keyboard, 4));
        Stored(order, keyboard);

        var result = await handler.HandleAsync(new CancelOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(OrderStatus.Canceled, result.Status);
        Assert.Equal(10, keyboard.AvailableQuantity);
    }

    [Fact]
    public async Task Execute_CalledTwice_ReleasesStockOnlyOnce()
    {
        var keyboard = Samples.Product(availableQuantity: 10);
        keyboard.Reserve(4);
        var order = Samples.ConfirmedOrder((keyboard, 4));
        Stored(order, keyboard);

        await handler.HandleAsync(new CancelOrderCommand(order.Id), CancellationToken.None);
        await handler.HandleAsync(new CancelOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(10, keyboard.AvailableQuantity);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_UnknownOrder_ThrowsNotFound()
    {
        orders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CancelOrderCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
