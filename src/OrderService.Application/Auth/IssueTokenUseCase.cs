using FluentValidation;
using OrderService.Application.Abstractions;

namespace OrderService.Application.Auth;

public sealed class IssueTokenUseCase(IUserStore users, ITokenIssuer tokens, IValidator<IssueTokenCommand> validator)
{
    public async Task<TokenDto> ExecuteAsync(IssueTokenCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = users.Validate(command.Username, command.Password)
            ?? throw new InvalidCredentialsException();

        return tokens.Issue(user);
    }
}
