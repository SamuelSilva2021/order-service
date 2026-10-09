using System.Net;
using OrderService.Api.IntegrationTests.Support;
using OrderService.Application.Common;
using OrderService.Application.Orders;
using OrderService.Domain.Orders;

namespace OrderService.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class OrderOwnershipTests(ApiFixture fixture)
{
    [Fact]
    public async Task Customer_PlacingOrderForAnotherCustomer_IsForbidden()
    {
        var admin = await fixture.CreateAdminAsync();
        var customer = await fixture.CreateCustomerAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);

        var response = await customer.PlaceOrderAsync(ApiFixture.CustomerTwoId, (product.Id, 1));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanPlaceOrderOnBehalfOfAnyCustomer()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);

        var order = await admin.PlaceOrderOkAsync(ApiFixture.CustomerTwoId, (product.Id, 1));

        Assert.Equal(ApiFixture.CustomerTwoId, order.CustomerId);
    }

    [Fact]
    public async Task Customer_ReadingConfirmingOrCancelingAnotherCustomersOrder_GetsNotFound()
    {
        var admin = await fixture.CreateAdminAsync();
        var first = await fixture.CreateCustomerAsync();
        var second = await fixture.CreateSecondCustomerAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var order = await first.PlaceOrderOkAsync(ApiFixture.CustomerOneId, (product.Id, 1));

        Assert.Equal(HttpStatusCode.NotFound, (await second.GetOrderAsync(order.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await second.ConfirmAsync(order.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await second.CancelAsync(order.Id)).StatusCode);
        Assert.Equal(OrderStatus.Placed, await admin.StatusOfAsync(order.Id));
    }

    [Fact]
    public async Task Customer_CanManageOwnOrder()
    {
        var admin = await fixture.CreateAdminAsync();
        var customer = await fixture.CreateCustomerAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var order = await customer.PlaceOrderOkAsync(ApiFixture.CustomerOneId, (product.Id, 1));

        Assert.Equal(HttpStatusCode.OK, (await customer.GetOrderAsync(order.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await customer.ConfirmAsync(order.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await customer.CancelAsync(order.Id)).StatusCode);
    }

    [Fact]
    public async Task Customer_ListingOrders_SeesOnlyOwn()
    {
        var admin = await fixture.CreateAdminAsync();
        var first = await fixture.CreateCustomerAsync();
        var second = await fixture.CreateSecondCustomerAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 50);
        await first.PlaceOrderOkAsync(ApiFixture.CustomerOneId, (product.Id, 1));
        await second.PlaceOrderOkAsync(ApiFixture.CustomerTwoId, (product.Id, 1));

        var response = await first.ListOrdersAsync("pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await ApiClient.ReadAsync<PagedResult<OrderSummaryDto>>(response);
        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, item => Assert.Equal(ApiFixture.CustomerOneId, item.CustomerId));
    }

    [Fact]
    public async Task Customer_ListingAnotherCustomersOrders_IsForbidden()
    {
        var customer = await fixture.CreateCustomerAsync();

        var response = await customer.ListOrdersAsync($"customerId={ApiFixture.CustomerTwoId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_ListingWithoutFilter_SeesOrdersOfSeveralCustomers()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 50);
        await admin.PlaceOrderOkAsync(ApiFixture.CustomerOneId, (product.Id, 1));
        await admin.PlaceOrderOkAsync(ApiFixture.CustomerTwoId, (product.Id, 1));

        var page = await ApiClient.ReadAsync<PagedResult<OrderSummaryDto>>(await admin.ListOrdersAsync("pageSize=100"));

        Assert.Contains(page.Items, item => item.CustomerId == ApiFixture.CustomerOneId);
        Assert.Contains(page.Items, item => item.CustomerId == ApiFixture.CustomerTwoId);
    }
}
