using System.Net;
using System.Net.Http.Json;
using OrderService.Api.IntegrationTests.Support;
using OrderService.Application.Auth;

namespace OrderService.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class AuthAndAuthorizationTests(ApiFixture fixture)
{
    [Theory]
    [InlineData("/orders")]
    [InlineData("/products")]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized(string path)
    {
        var response = await fixture.CreateAnonymous().Http.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task IssueToken_ValidCredentials_ReturnsToken()
    {
        var response = await fixture.CreateAnonymous().RequestTokenAsync("admin", "admin123");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await ApiClient.ReadAsync<TokenDto>(response);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("admin", "wrong")]
    [InlineData("ghost", "admin123")]
    public async Task IssueToken_InvalidCredentials_ReturnsUnauthorized(string username, string password)
    {
        var response = await fixture.CreateAnonymous().RequestTokenAsync(username, password);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task IssueToken_MissingFields_ReturnsBadRequest()
    {
        var response = await fixture.CreateAnonymous().RequestTokenAsync("", "");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AsCustomer_ReturnsForbidden()
    {
        var customer = await fixture.CreateCustomerAsync();

        var response = await customer.Http.PostAsJsonAsync(
            "/products",
            new { name = "Forbidden", unitPrice = 10, currency = "BRL", availableQuantity = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AsAdmin_ReturnsCreated()
    {
        var admin = await fixture.CreateAdminAsync();

        var product = await admin.CreateProductAsync(availableQuantity: 7, unitPrice: 19.9m);

        Assert.Equal(7, product.AvailableQuantity);
        Assert.Equal(19.9m, product.UnitPrice);
    }

    [Fact]
    public async Task Customer_CanPlaceOrders()
    {
        var admin = await fixture.CreateAdminAsync();
        var customer = await fixture.CreateCustomerAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);

        var order = await customer.PlaceOrderOkAsync(Guid.NewGuid().ToString(), (product.Id, 1));

        Assert.Equal(OrderService.Domain.Orders.OrderStatus.Placed, order.Status);
    }
}
