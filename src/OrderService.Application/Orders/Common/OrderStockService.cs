using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Orders;
using OrderService.Domain.Products;

namespace OrderService.Application.Orders.Common;

public sealed class OrderStockService(IProductRepository products)
{
    public Task ReserveAsync(Order order, CancellationToken cancellationToken) =>
        AdjustAsync(order, (product, item) => product.Reserve(item.Quantity), cancellationToken);

    public Task ReleaseAsync(Order order, CancellationToken cancellationToken) =>
        AdjustAsync(order, (product, item) => product.Release(item.Quantity), cancellationToken);

    private async Task AdjustAsync(Order order, Action<Product, OrderItem> adjustment, CancellationToken cancellationToken)
    {
        var ids = order.Items.Select(item => item.ProductId).ToList();
        var catalog = (await products.GetByIdsAsync(ids, cancellationToken)).ToDictionary(product => product.Id);

        foreach (var item in order.Items)
        {
            adjustment(catalog[item.ProductId], item);
        }
    }
}
