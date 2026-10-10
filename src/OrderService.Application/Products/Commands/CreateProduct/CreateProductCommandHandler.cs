using FluentValidation;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Products.Common;
using OrderService.Domain.Common;
using OrderService.Domain.Products;

namespace OrderService.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductCommand> validator)
{
    public async Task<ProductDto> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
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
