using OrderService.Domain.Products;

namespace OrderService.Application.Products.Common;

public sealed record ProductDto(Guid Id, string Name, decimal UnitPrice, string Currency, int AvailableQuantity)
{
    public static ProductDto From(Product product) =>
        new(product.Id, product.Name, product.UnitPrice, product.Currency.Code, product.AvailableQuantity);
}
