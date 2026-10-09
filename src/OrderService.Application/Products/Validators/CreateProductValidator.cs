using FluentValidation;
using OrderService.Application.Products.Requests;

namespace OrderService.Application.Products.Validators;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do produto deve ter no máximo 200 caracteres.");

        RuleFor(command => command.UnitPrice)
            .GreaterThan(0).WithMessage("O preço unitário deve ser maior que zero.");

        RuleFor(command => command.Currency)
            .NotEmpty().WithMessage("A moeda é obrigatória.")
            .Length(3).WithMessage("A moeda deve ter exatamente 3 letras.");

        RuleFor(command => command.AvailableQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade disponível não pode ser negativa.");
    }
}
