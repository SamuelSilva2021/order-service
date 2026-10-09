using FluentValidation;
using FluentValidation.Results;
using OrderService.Application.Abstractions;
using OrderService.Domain.Common;
using OrderService.Domain.Orders;

namespace OrderService.Application.Orders;

public sealed class CreateOrderUseCase(
    IOrderRepository orders,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IValidator<CreateOrderCommand> validator,
    OrderAccessPolicy access,
    TimeProvider clock)
{
    public async Task<OrderDto> ExecuteAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        access.EnsureCanPlaceFor(command.CustomerId);

        var currency = Currency.Of(command.Currency);
        var productIds = command.Items.Select(item => item.ProductId).ToList();
        var catalog = (await products.GetByIdsAsync(productIds, cancellationToken)).ToDictionary(product => product.Id);

        EnsureAllProductsExist(productIds, catalog.Keys);

        var items = command.Items.Select(item => catalog[item.ProductId].ToOrderItem(item.Quantity, currency));
        var order = Order.Place(command.CustomerId, currency, items, clock.GetUtcNow());

        orders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderDto.From(order);
    }

    private static void EnsureAllProductsExist(IEnumerable<Guid> requested, IEnumerable<Guid> existing)
    {
        var missing = requested.Except(existing).ToList();

        if (missing.Count > 0)
        {
            var failure = new ValidationFailure("items", $"Unknown product(s): {string.Join(", ", missing)}.");
            throw new ValidationException([failure]);
        }
    }
}
