using FluentValidation;
using OrderService.Application.Abstractions;
using OrderService.Application.Common;

namespace OrderService.Application.Orders;

public sealed class ListOrdersUseCase(IOrderQueries queries, IValidator<ListOrdersQuery> validator)
{
    public async Task<PagedResult<OrderSummaryDto>> ExecuteAsync(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        return await queries.SearchAsync(query, cancellationToken);
    }
}
