using FluentValidation;
using OrderService.Application.Abstractions;
using OrderService.Domain.Common;
using OrderService.Domain.Products;

namespace OrderService.Application.Products;

public sealed class CreateProductUseCase(
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductCommand> validator)
{
    public async Task<ProductDto> ExecuteAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var product = Product.Create(
            command.Name,
            command.UnitPrice,
            Currency.Of(command.Currency),
            command.AvailableQuantity);

        products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDto.From(product);
    }
}
