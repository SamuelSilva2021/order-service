using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstractions;
using OrderService.Application.Common;
using OrderService.Application.Products;

namespace OrderService.Infrastructure.Persistence.Queries;

public sealed class ProductQueries(AppDbContext context) : IProductQueries
{
    public async Task<PagedResult<ProductDto>> ListAsync(ListProductsQuery query, CancellationToken cancellationToken)
    {
        var products = context.Products.AsNoTracking();

        var totalCount = await products.CountAsync(cancellationToken);

        var items = await products
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.UnitPrice,
                product.Currency.Code,
                product.AvailableQuantity))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>(items, query.Page, query.PageSize, totalCount);
    }
}
