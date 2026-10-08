using OrderService.Application.Common;
using OrderService.Application.Products;

namespace OrderService.Application.Abstractions;

public interface IProductQueries
{
    Task<PagedResult<ProductDto>> ListAsync(ListProductsQuery query, CancellationToken cancellationToken);
}
