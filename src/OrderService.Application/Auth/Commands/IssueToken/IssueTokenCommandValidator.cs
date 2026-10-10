using FluentValidation;

namespace OrderService.Application.Auth.Commands.IssueToken;

public sealed class IssueTokenCommandValidator : AbstractValidator<IssueTokenCommand>
{
    public IssueTokenCommandValidator()
    {
        RuleFor(command => command.Username).NotEmpty().WithMessage("O usuário é obrigatório.");
        RuleFor(command => command.Password).NotEmpty().WithMessage("A senha é obrigatória.");
    }
}
