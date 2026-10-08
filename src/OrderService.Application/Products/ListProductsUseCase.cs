using FluentValidation;
using OrderService.Application.Abstractions;
using OrderService.Application.Common;

namespace OrderService.Application.Products;

public sealed class ListProductsUseCase(IProductQueries queries, IValidator<ListProductsQuery> validator)
{
    public async Task<PagedResult<ProductDto>> ExecuteAsync(ListProductsQuery query, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        return await queries.ListAsync(query, cancellationToken);
    }
}
