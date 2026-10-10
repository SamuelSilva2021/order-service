using NSubstitute;
using OrderService.Application.Common.Exceptions;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Common.Models;
using OrderService.Application.Orders.Commands.CancelOrder;
using OrderService.Application.Orders.Commands.ConfirmOrder;
using OrderService.Application.Orders.Common;
using OrderService.Application.Orders.Queries.GetOrder;
using OrderService.Application.Orders.Queries.ListOrders;
using OrderService.Application.Tests.Support;
using OrderService.Domain.Orders;

namespace OrderService.Application.Tests.Orders.Common;

public class OrderOwnershipTests
{
    private readonly IOrderRepository orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private Order StoredOrderOf(string customerId)
    {
        var product = Samples.Product();
        var order = Order.Place(customerId, Samples.Brl, [new OrderItem(product.Id, 10m, 1)], DateTimeOffset.UtcNow);
        orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        products.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns([product]);
        return order;
    }

    private ConfirmOrderCommandHandler ConfirmFor(FakeCurrentUser user) =>
        new(orders, new OrderStockService(products), new OrderAccessPolicy(user), unitOfWork);

    private CancelOrderCommandHandler CancelFor(FakeCurrentUser user) =>
        new(orders, new OrderStockService(products), new OrderAccessPolicy(user), unitOfWork);

    [Fact]
    public async Task Confirm_OrderOfAnotherCustomer_ThrowsNotFoundAndDoesNotSave()
    {
        var order = StoredOrderOf("customer-2");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            ConfirmFor(FakeCurrentUser.Customer("customer-1")).HandleAsync(new ConfirmOrderCommand(order.Id), CancellationToken.None));
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(OrderStatus.Placed, order.Status);
    }

    [Fact]
    public async Task Confirm_OwnOrder_Succeeds()
    {
        var order = StoredOrderOf("customer-1");

        var result = await ConfirmFor(FakeCurrentUser.Customer("customer-1")).HandleAsync(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(OrderStatus.Confirmed, result.Status);
    }

    [Fact]
    public async Task Confirm_AdminOnAnyOrder_Succeeds()
    {
        var order = StoredOrderOf("customer-2");

        var result = await ConfirmFor(FakeCurrentUser.Admin).HandleAsync(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(OrderStatus.Confirmed, result.Status);
    }

    [Fact]
    public async Task Cancel_OrderOfAnotherCustomer_ThrowsNotFoundAndKeepsStatus()
    {
        var order = StoredOrderOf("customer-2");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CancelFor(FakeCurrentUser.Customer("customer-1")).HandleAsync(new CancelOrderCommand(order.Id), CancellationToken.None));
        Assert.Equal(OrderStatus.Placed, order.Status);
    }

    [Fact]
    public async Task Get_OrderOfAnotherCustomer_ThrowsNotFound()
    {
        var queries = Substitute.For<IOrderQueries>();
        var dto = new OrderDto(Guid.NewGuid(), "customer-2", OrderStatus.Placed, "BRL", 10m, DateTimeOffset.UtcNow, []);
        queries.GetByIdAsync(dto.Id, Arg.Any<CancellationToken>()).Returns(dto);
        var handler = new GetOrderQueryHandler(queries, new OrderAccessPolicy(FakeCurrentUser.Customer("customer-1")));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new GetOrderQuery(dto.Id), CancellationToken.None));
    }

    [Fact]
    public async Task List_Customer_AlwaysScopedToOwnCustomerId()
    {
        var queries = Substitute.For<IOrderQueries>();
        queries.SearchAsync(Arg.Any<ListOrdersQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<OrderSummaryDto>([], 1, 20, 0));
        var handler = new ListOrdersQueryHandler(
            queries,
            new OrderAccessPolicy(FakeCurrentUser.Customer("customer-1")),
            new ListOrdersQueryValidator());

        await handler.HandleAsync(new ListOrdersQuery(), CancellationToken.None);

        await queries.Received(1).SearchAsync(
            Arg.Is<ListOrdersQuery>(query => query.CustomerId == "customer-1"),
            Arg.Any<CancellationToken>());
    }
}
