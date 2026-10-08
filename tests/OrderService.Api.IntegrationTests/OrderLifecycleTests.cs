using System.Net;
using OrderService.Api.IntegrationTests.Support;
using OrderService.Domain.Orders;

namespace OrderService.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class OrderLifecycleTests(ApiFixture fixture)
{
    private static string NewCustomer() => $"customer-{Guid.NewGuid():N}";

    [Fact]
    public async Task Confirm_PlacedOrder_ConfirmsAndReservesStock()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var order = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 5));

        var response = await admin.ConfirmAsync(order.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(OrderStatus.Confirmed, await admin.StatusOfAsync(order.Id));
        var next = await admin.PlaceOrderAsync(NewCustomer(), (product.Id, 1));
        Assert.Equal(HttpStatusCode.Conflict, next.StatusCode);
    }

    [Fact]
    public async Task Confirm_CalledTwice_IsIdempotentAndReservesOnce()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var order = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 3));

        var first = await admin.ConfirmAsync(order.Id);
        var second = await admin.ConfirmAsync(order.Id);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(OrderStatus.Confirmed, await admin.StatusOfAsync(order.Id));
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PlaceOrderAsync(NewCustomer(), (product.Id, 3))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await admin.PlaceOrderAsync(NewCustomer(), (product.Id, 2))).StatusCode);
    }

    [Fact]
    public async Task Confirm_StockConsumedByAnotherOrder_ReturnsConflict()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var first = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 4));
        var second = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 4));
        await admin.ConfirmAsync(first.Id);

        var response = await admin.ConfirmAsync(second.Id);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(OrderStatus.Placed, await admin.StatusOfAsync(second.Id));
    }

    [Fact]
    public async Task Confirm_ConcurrentOrdersCompetingForStock_ConfirmsOnlyOne()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var first = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 3));
        var second = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 3));

        var responses = await Task.WhenAll(admin.ConfirmAsync(first.Id), admin.ConfirmAsync(second.Id));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Confirm_UnknownOrder_ReturnsNotFound()
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.ConfirmAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Confirm_CanceledOrder_ReturnsConflict()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var order = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 1));
        await admin.CancelAsync(order.Id);

        var response = await admin.ConfirmAsync(order.Id);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_PlacedOrder_CancelsAndKeepsStock()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var order = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 5));

        var response = await admin.CancelAsync(order.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(OrderStatus.Canceled, await admin.StatusOfAsync(order.Id));
        Assert.Equal(HttpStatusCode.Created, (await admin.PlaceOrderAsync(NewCustomer(), (product.Id, 5))).StatusCode);
    }

    [Fact]
    public async Task Cancel_ConfirmedOrder_ReleasesStock()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var order = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 5));
        await admin.ConfirmAsync(order.Id);

        var response = await admin.CancelAsync(order.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await admin.PlaceOrderAsync(NewCustomer(), (product.Id, 5))).StatusCode);
    }

    [Fact]
    public async Task Cancel_CalledTwice_IsIdempotentAndReleasesOnce()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var order = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 3));
        await admin.ConfirmAsync(order.Id);

        var first = await admin.CancelAsync(order.Id);
        var second = await admin.CancelAsync(order.Id);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PlaceOrderAsync(NewCustomer(), (product.Id, 6))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await admin.PlaceOrderAsync(NewCustomer(), (product.Id, 5))).StatusCode);
    }
}
