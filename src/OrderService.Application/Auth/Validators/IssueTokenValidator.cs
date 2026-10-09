using FluentValidation;
using OrderService.Application.Auth.Requests;

namespace OrderService.Application.Auth.Validators;

public sealed class IssueTokenValidator : AbstractValidator<IssueTokenCommand>
{
    public IssueTokenValidator()
    {
        RuleFor(command => command.Username).NotEmpty().WithMessage("O usuário é obrigatório.");
        RuleFor(command => command.Password).NotEmpty().WithMessage("A senha é obrigatória.");
    }
}
