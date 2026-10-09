using OrderService.Application.Common;
using OrderService.Application.Products.Dtos;
using OrderService.Application.Products.Requests;

namespace OrderService.Application.Abstractions;

public interface IProductQueries
{
    Task<PagedResult<ProductDto>> ListAsync(ListProductsQuery query, CancellationToken cancellationToken);
}
