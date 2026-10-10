using FluentValidation;
using NSubstitute;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Orders.Commands.CreateOrder;
using OrderService.Application.Orders.Common;
using OrderService.Application.Tests.Support;
using OrderService.Domain.Common;
using OrderService.Domain.Orders;
using OrderService.Domain.Products;

namespace OrderService.Application.Tests.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateOrderCommandHandler handler;

    public CreateOrderCommandHandlerTests()
    {
        handler = CreateHandler(FakeCurrentUser.Admin);
    }

    private CreateOrderCommandHandler CreateHandler(ICurrentUser user) =>
        new(orders, products, unitOfWork, new CreateOrderCommandValidator(), new OrderAccessPolicy(user), TimeProvider.System);

    private void CatalogContains(params Product[] catalog) =>
        products.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(call => catalog.Where(product => call.Arg<IReadOnlyCollection<Guid>>().Contains(product.Id)).ToList());

    private static CreateOrderCommand Command(params (Product Product, int Quantity)[] lines) =>
        new("customer-1", "BRL", lines.Select(line => new CreateOrderItemCommand(line.Product.Id, line.Quantity)).ToList());

    [Fact]
    public async Task Execute_ValidCommand_PlacesOrderWithComputedTotal()
    {
        var keyboard = Samples.Product(unitPrice: 100m);
        var mouse = Samples.Product(unitPrice: 25.5m);
        CatalogContains(keyboard, mouse);

        var order = await handler.HandleAsync(Command((keyboard, 2), (mouse, 3)), CancellationToken.None);

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal(276.5m, order.Total);
        Assert.Equal(2, order.Items.Count);
        orders.Received(1).Add(Arg.Any<Order>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DoesNotConsumeStock()
    {
        var keyboard = Samples.Product(availableQuantity: 5);
        CatalogContains(keyboard);

        await handler.HandleAsync(Command((keyboard, 2)), CancellationToken.None);

        Assert.Equal(5, keyboard.AvailableQuantity);
    }

    [Fact]
    public async Task Execute_WithoutItems_FailsValidation()
    {
        var command = new CreateOrderCommand("customer-1", "BRL", []);

        await Assert.ThrowsAsync<ValidationException>(() => handler.HandleAsync(command, CancellationToken.None));
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Execute_NonPositiveQuantity_FailsValidation(int quantity)
    {
        var keyboard = Samples.Product();

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(Command((keyboard, quantity)), CancellationToken.None));
    }

    [Fact]
    public async Task Execute_RepeatedProduct_FailsValidation()
    {
        var keyboard = Samples.Product();

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(Command((keyboard, 1), (keyboard, 2)), CancellationToken.None));
    }

    [Fact]
    public async Task Execute_UnknownProduct_FailsValidation()
    {
        var unknown = Samples.Product();
        CatalogContains();

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(Command((unknown, 1)), CancellationToken.None));
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_QuantityAboveStock_ThrowsInsufficientStock()
    {
        var keyboard = Samples.Product(availableQuantity: 2);
        CatalogContains(keyboard);

        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            handler.HandleAsync(Command((keyboard, 3)), CancellationToken.None));
        orders.DidNotReceive().Add(Arg.Any<Order>());
    }

    [Fact]
    public async Task Execute_CurrencyDifferentFromProduct_ThrowsDomainException()
    {
        var keyboard = Samples.Product();
        CatalogContains(keyboard);
        var command = Command((keyboard, 1)) with { Currency = "USD" };

        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command, CancellationToken.None));
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
