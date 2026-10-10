using FluentValidation;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Common.Models;
using OrderService.Application.Products.Common;

namespace OrderService.Application.Products.Queries.ListProducts;

public sealed class ListProductsQueryHandler(IProductQueries queries, IValidator<ListProductsQuery> validator)
{
    public async Task<PagedResult<ProductDto>> HandleAsync(ListProductsQuery query, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        return await queries.ListAsync(query, cancellationToken);
    }
}
