using FluentValidation;

namespace OrderService.Application.Orders;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(command => command.CustomerId)
            .NotEmpty().WithMessage("O customerId é obrigatório.")
            .MaximumLength(64).WithMessage("O customerId deve ter no máximo 64 caracteres.");

        RuleFor(command => command.Currency)
            .NotEmpty().WithMessage("A moeda é obrigatória.")
            .Length(3).WithMessage("A moeda deve ter exatamente 3 letras.");

        RuleFor(command => command.Items)
            .NotEmpty().WithMessage("O pedido deve ter ao menos um item.");

        RuleForEach(command => command.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("O productId é obrigatório.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        });

        RuleFor(command => command.Items)
            .Must(items => items.Select(item => item.ProductId).Distinct().Count() == items.Count)
            .When(command => command.Items is not null)
            .WithMessage("Cada produto pode aparecer apenas uma vez por pedido.");
    }
}
