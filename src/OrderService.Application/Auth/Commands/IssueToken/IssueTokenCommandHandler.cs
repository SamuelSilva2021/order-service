using FluentValidation;
using OrderService.Application.Auth.Common;
using OrderService.Application.Common.Interfaces;

namespace OrderService.Application.Auth.Commands.IssueToken;

public sealed class IssueTokenCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokens,
    IValidator<IssueTokenCommand> validator)
{
    public async Task<TokenDto> HandleAsync(IssueTokenCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await users.FindByUsernameAsync(command.Username, cancellationToken);
        var passwordMatches = passwordHasher.Verify(user?.PasswordHash ?? string.Empty, command.Password);

        if (user is null || !passwordMatches)
            throw new InvalidCredentialsException();

        return tokens.Issue(new AuthenticatedUser(user.Username, user.Role.ToString(), user.CustomerId));
    }
}
