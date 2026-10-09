using FluentValidation;
using OrderService.Application.Abstractions;
using OrderService.Application.Auth.Dtos;
using OrderService.Application.Auth.Requests;

namespace OrderService.Application.Auth.UseCases;

public sealed class IssueTokenUseCase(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokens,
    IValidator<IssueTokenCommand> validator)
{
    public async Task<TokenDto> ExecuteAsync(IssueTokenCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await users.FindByUsernameAsync(command.Username, cancellationToken);
        var passwordMatches = passwordHasher.Verify(user?.PasswordHash ?? string.Empty, command.Password);

        if (user is null || !passwordMatches)
            throw new InvalidCredentialsException();

        return tokens.Issue(new AuthenticatedUser(user.Username, user.Role.ToString(), user.CustomerId));
    }
}
