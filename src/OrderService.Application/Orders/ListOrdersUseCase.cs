using FluentValidation;
using OrderService.Application.Abstractions;
using OrderService.Application.Common;

namespace OrderService.Application.Orders;

public sealed class ListOrdersUseCase(IOrderQueries queries, OrderAccessPolicy access, IValidator<ListOrdersQuery> validator)
{
    public async Task<PagedResult<OrderSummaryDto>> ExecuteAsync(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var scopedQuery = query with { CustomerId = access.ResolveCustomerFilter(query.CustomerId) };

        return await queries.SearchAsync(scopedQuery, cancellationToken);
    }
}
