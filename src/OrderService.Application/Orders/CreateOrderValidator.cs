using FluentValidation;

namespace OrderService.Application.Orders;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty().MaximumLength(64);
        RuleFor(command => command.Currency).NotEmpty().Length(3);
        RuleFor(command => command.Items).NotEmpty();

        RuleForEach(command => command.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });

        RuleFor(command => command.Items)
            .Must(items => items.Select(item => item.ProductId).Distinct().Count() == items.Count)
            .When(command => command.Items is not null)
            .WithMessage("Each product can appear only once per order.");
    }
}
