using FluentValidation;

namespace OrderService.Application.Auth;

public sealed class IssueTokenValidator : AbstractValidator<IssueTokenCommand>
{
    public IssueTokenValidator()
    {
        RuleFor(command => command.Username).NotEmpty();
        RuleFor(command => command.Password).NotEmpty();
    }
}
