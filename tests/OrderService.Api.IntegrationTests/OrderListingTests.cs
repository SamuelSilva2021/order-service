using System.Net;
using OrderService.Api.IntegrationTests.Support;
using OrderService.Application.Common.Models;
using OrderService.Application.Orders.Common;
using OrderService.Application.Orders.Queries.ListOrders;
using OrderService.Domain.Orders;

namespace OrderService.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class OrderListingTests(ApiFixture fixture)
{
    private async Task<(ApiClient Admin, string CustomerId, OrderDto[] Orders)> SeedOrdersAsync(int count)
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 1000);
        var customerId = $"customer-{Guid.NewGuid():N}";
        var orders = new List<OrderDto>();

        for (var index = 0; index < count; index++)
        {
            orders.Add(await admin.PlaceOrderOkAsync(customerId, (product.Id, 1)));
        }

        return (admin, customerId, orders.ToArray());
    }

    private static async Task<PagedResult<OrderSummaryDto>> ReadPageAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ApiClient.ReadAsync<PagedResult<OrderSummaryDto>>(response);
    }

    [Fact]
    public async Task List_FiltersByCustomer()
    {
        var (admin, customerId, _) = await SeedOrdersAsync(3);
        await SeedOrdersAsync(2);

        var page = await ReadPageAsync(await admin.ListOrdersAsync($"customerId={customerId}"));

        Assert.Equal(3, page.TotalCount);
        Assert.All(page.Items, item => Assert.Equal(customerId, item.CustomerId));
    }

    [Fact]
    public async Task List_PaginatesNewestFirst()
    {
        var (admin, customerId, orders) = await SeedOrdersAsync(3);

        var first = await ReadPageAsync(await admin.ListOrdersAsync($"customerId={customerId}&page=1&pageSize=2"));
        var second = await ReadPageAsync(await admin.ListOrdersAsync($"customerId={customerId}&page=2&pageSize=2"));

        Assert.Equal(2, first.Items.Count);
        Assert.Single(second.Items);
        Assert.Equal(3, first.TotalCount);
        Assert.Equal(2, first.TotalPages);
        Assert.Equal(orders[2].Id, first.Items[0].Id);
        Assert.Equal(orders[0].Id, second.Items[0].Id);
    }

    [Fact]
    public async Task List_FiltersByStatus()
    {
        var (admin, customerId, orders) = await SeedOrdersAsync(3);
        await admin.ConfirmAsync(orders[0].Id);
        await admin.CancelAsync(orders[1].Id);

        var confirmed = await ReadPageAsync(await admin.ListOrdersAsync($"customerId={customerId}&status=Confirmed"));
        var placed = await ReadPageAsync(await admin.ListOrdersAsync($"customerId={customerId}&status=Placed"));

        Assert.Equal([orders[0].Id], confirmed.Items.Select(item => item.Id));
        Assert.Equal([orders[2].Id], placed.Items.Select(item => item.Id));
        Assert.All(confirmed.Items, item => Assert.Equal(OrderStatus.Confirmed, item.Status));
    }

    [Fact]
    public async Task List_FiltersByCreationInterval()
    {
        var (admin, customerId, orders) = await SeedOrdersAsync(2);
        var from = Uri.EscapeDataString(orders[0].CreatedAt.AddMinutes(-1).ToString("o"));
        var tomorrow = Uri.EscapeDataString(orders[1].CreatedAt.AddDays(1).ToString("o"));
        var future = Uri.EscapeDataString(orders[1].CreatedAt.AddMinutes(1).ToString("o"));

        var inside = await ReadPageAsync(await admin.ListOrdersAsync($"customerId={customerId}&from={from}&to={tomorrow}"));
        var after = await ReadPageAsync(await admin.ListOrdersAsync($"customerId={customerId}&from={future}"));

        Assert.Equal(2, inside.TotalCount);
        Assert.Equal(0, after.TotalCount);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    [InlineData("status=Unknown")]
    [InlineData("from=2030-01-02T00:00:00Z&to=2030-01-01T00:00:00Z")]
    public async Task List_InvalidParameters_ReturnsBadRequest(string query)
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.ListOrdersAsync(query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
