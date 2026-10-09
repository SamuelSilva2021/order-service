using System.Net;
using OrderService.Api.IntegrationTests.Support;
using OrderService.Application.Orders.Dtos;
using OrderService.Domain.Orders;

namespace OrderService.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class OrderCreationTests(ApiFixture fixture)
{
    private static string NewCustomer() => $"customer-{Guid.NewGuid():N}";

    [Fact]
    public async Task Create_ValidOrder_ReturnsPlacedOrderWithTotalAndLocation()
    {
        var admin = await fixture.CreateAdminAsync();
        var keyboard = await admin.CreateProductAsync(availableQuantity: 10, unitPrice: 349.90m);
        var mouse = await admin.CreateProductAsync(availableQuantity: 10, unitPrice: 129.90m);

        var response = await admin.PlaceOrderAsync(NewCustomer(), (keyboard.Id, 2), (mouse.Id, 1));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await ApiClient.ReadAsync<OrderDto>(response);
        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal(829.70m, order.Total);
        Assert.Equal(2, order.Items.Count);
        Assert.EndsWith($"/orders/{order.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Get_ExistingOrder_ReturnsOrderWithItems()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 10, unitPrice: 50m);
        var created = await admin.PlaceOrderOkAsync(NewCustomer(), (product.Id, 3));

        var response = await admin.GetOrderAsync(created.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await ApiClient.ReadAsync<OrderDto>(response);
        Assert.Equal(created.Id, order.Id);
        Assert.Equal(150m, order.Total);
        var item = Assert.Single(order.Items);
        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(150m, item.Subtotal);
    }

    [Fact]
    public async Task Get_UnknownOrder_ReturnsNotFound()
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.GetOrderAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutItems_ReturnsBadRequestWithErrors()
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.PlaceOrderAsync(NewCustomer());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("errors", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Create_NonPositiveQuantity_ReturnsBadRequest(int quantity)
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 10);

        var response = await admin.PlaceOrderAsync(NewCustomer(), (product.Id, quantity));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_UnknownProduct_ReturnsBadRequest()
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.PlaceOrderAsync(NewCustomer(), (Guid.NewGuid(), 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_QuantityAboveStock_ReturnsConflict()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 2);

        var response = await admin.PlaceOrderAsync(NewCustomer(), (product.Id, 3));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_QuantityEqualToStock_Succeeds()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 2);

        var response = await admin.PlaceOrderAsync(NewCustomer(), (product.Id, 2));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_SeededProduct_IsAvailable()
    {
        var admin = await fixture.CreateAdminAsync();
        var keyboard = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var response = await admin.PlaceOrderAsync(NewCustomer(), (keyboard, 1));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
