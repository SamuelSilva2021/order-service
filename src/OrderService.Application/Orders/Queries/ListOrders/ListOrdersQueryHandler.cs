using FluentValidation;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Common.Models;
using OrderService.Application.Orders.Common;

namespace OrderService.Application.Orders.Queries.ListOrders;

public sealed class ListOrdersQueryHandler(IOrderQueries queries, OrderAccessPolicy access, IValidator<ListOrdersQuery> validator)
{
    public async Task<PagedResult<OrderSummaryDto>> HandleAsync(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var scopedQuery = query with { CustomerId = access.ResolveCustomerFilter(query.CustomerId) };

        return await queries.SearchAsync(scopedQuery, cancellationToken);
    }
}
