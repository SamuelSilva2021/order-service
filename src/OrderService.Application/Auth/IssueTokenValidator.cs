using FluentValidation;

namespace OrderService.Application.Auth;

public sealed class IssueTokenValidator : AbstractValidator<IssueTokenCommand>
{
    public IssueTokenValidator()
    {
        RuleFor(command => command.Username).NotEmpty().WithMessage("O usuário é obrigatório.");
        RuleFor(command => command.Password).NotEmpty().WithMessage("A senha é obrigatória.");
    }
}
