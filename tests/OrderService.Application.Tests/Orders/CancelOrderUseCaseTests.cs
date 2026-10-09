using NSubstitute;
using OrderService.Application.Abstractions;
using OrderService.Application.Common;
using OrderService.Application.Orders;
using OrderService.Application.Tests.Support;
using OrderService.Domain.Orders;
using OrderService.Domain.Products;

namespace OrderService.Application.Tests.Orders;

public class CancelOrderUseCaseTests
{
    private readonly IOrderRepository orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CancelOrderUseCase useCase;

    public CancelOrderUseCaseTests()
    {
        useCase = CreateUseCase(FakeCurrentUser.Admin);
    }

    private CancelOrderUseCase CreateUseCase(ICurrentUser user) =>
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

        var result = await useCase.ExecuteAsync(order.Id, CancellationToken.None);

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

        var result = await useCase.ExecuteAsync(order.Id, CancellationToken.None);

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

        await useCase.ExecuteAsync(order.Id, CancellationToken.None);
        await useCase.ExecuteAsync(order.Id, CancellationToken.None);

        Assert.Equal(10, keyboard.AvailableQuantity);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_UnknownOrder_ThrowsNotFound()
    {
        orders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.ExecuteAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
