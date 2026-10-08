using FluentValidation;

namespace OrderService.Application.Products;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.UnitPrice).GreaterThan(0);
        RuleFor(command => command.Currency).NotEmpty().Length(3);
        RuleFor(command => command.AvailableQuantity).GreaterThanOrEqualTo(0);
    }
}
