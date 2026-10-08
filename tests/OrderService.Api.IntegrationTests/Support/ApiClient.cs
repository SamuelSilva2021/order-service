using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrderService.Application.Auth;
using OrderService.Application.Common;
using OrderService.Application.Orders;
using OrderService.Application.Products;
using OrderService.Domain.Orders;

namespace OrderService.Api.IntegrationTests.Support;

public sealed class ApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public HttpClient Http => http;

    public async Task SignInAsync(string username, string password)
    {
        var response = await RequestTokenAsync(username, password);
        response.EnsureSuccessStatusCode();
        var token = await ReadAsync<TokenDto>(response);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    public Task<HttpResponseMessage> RequestTokenAsync(string username, string password) =>
        http.PostAsJsonAsync("/auth/token", new IssueTokenCommand(username, password), Json);

    public async Task<ProductDto> CreateProductAsync(int availableQuantity, decimal unitPrice = 100m)
    {
        var response = await http.PostAsJsonAsync(
            "/products",
            new CreateProductCommand($"Product {Guid.NewGuid():N}", unitPrice, "BRL", availableQuantity),
            Json);
        response.EnsureSuccessStatusCode();
        return await ReadAsync<ProductDto>(response);
    }

    public Task<HttpResponseMessage> PlaceOrderAsync(string customerId, params (Guid ProductId, int Quantity)[] items) =>
        http.PostAsJsonAsync(
            "/orders",
            new CreateOrderCommand(customerId, "BRL", items.Select(i => new CreateOrderItemCommand(i.ProductId, i.Quantity)).ToList()),
            Json);

    public async Task<OrderDto> PlaceOrderOkAsync(string customerId, params (Guid ProductId, int Quantity)[] items)
    {
        var response = await PlaceOrderAsync(customerId, items);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        return await ReadAsync<OrderDto>(response);
    }

    public Task<HttpResponseMessage> ConfirmAsync(Guid orderId) => http.PostAsync($"/orders/{orderId}/confirm", null);

    public Task<HttpResponseMessage> CancelAsync(Guid orderId) => http.PostAsync($"/orders/{orderId}/cancel", null);

    public Task<HttpResponseMessage> GetOrderAsync(Guid orderId) => http.GetAsync($"/orders/{orderId}");

    public Task<HttpResponseMessage> ListOrdersAsync(string query) => http.GetAsync($"/orders?{query}");

    public async Task<OrderStatus> StatusOfAsync(Guid orderId) =>
        (await ReadAsync<OrderDto>(await GetOrderAsync(orderId))).Status;

    public static Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        response.Content.ReadFromJsonAsync<T>(Json)!;
}
