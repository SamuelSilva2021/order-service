using FluentValidation;
using OrderService.Application.Orders.Requests;

namespace OrderService.Application.Orders.Validators;

public sealed class ListOrdersValidator : AbstractValidator<ListOrdersQuery>
{
    public ListOrdersValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1).WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100).WithMessage("O tamanho da página deve estar entre 1 e 100.");

        RuleFor(query => query.To)
            .GreaterThanOrEqualTo(query => query.From)
            .When(query => query.From.HasValue && query.To.HasValue)
            .WithMessage("A data final deve ser igual ou posterior à data inicial.");
    }
}
