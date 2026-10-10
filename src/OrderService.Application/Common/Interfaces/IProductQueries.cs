using OrderService.Application.Common.Models;
using OrderService.Application.Products.Common;
using OrderService.Application.Products.Queries.ListProducts;

namespace OrderService.Application.Common.Interfaces;

public interface IProductQueries
{
    Task<PagedResult<ProductDto>> ListAsync(ListProductsQuery query, CancellationToken cancellationToken);
}
