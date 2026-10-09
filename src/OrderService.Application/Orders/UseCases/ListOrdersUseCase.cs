using FluentValidation;
using OrderService.Application.Abstractions;
using OrderService.Application.Common;
using OrderService.Application.Orders.Dtos;
using OrderService.Application.Orders.Requests;
using OrderService.Application.Orders.Services;

namespace OrderService.Application.Orders.UseCases;

public sealed class ListOrdersUseCase(IOrderQueries queries, OrderAccessPolicy access, IValidator<ListOrdersQuery> validator)
{
    public async Task<PagedResult<OrderSummaryDto>> ExecuteAsync(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var scopedQuery = query with { CustomerId = access.ResolveCustomerFilter(query.CustomerId) };

        return await queries.SearchAsync(scopedQuery, cancellationToken);
    }
}
