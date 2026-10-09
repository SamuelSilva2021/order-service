using FluentValidation;
using NSubstitute;
using OrderService.Application.Abstractions;
using OrderService.Application.Auth;
using OrderService.Application.Auth.Dtos;
using OrderService.Application.Auth.Requests;
using OrderService.Application.Auth.UseCases;
using OrderService.Application.Auth.Validators;
using OrderService.Domain.Users;

namespace OrderService.Application.Tests.Auth;

public class IssueTokenUseCaseTests
{
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenIssuer tokens = Substitute.For<ITokenIssuer>();
    private readonly IssueTokenUseCase useCase;

    public IssueTokenUseCaseTests()
    {
        useCase = new IssueTokenUseCase(users, hasher, tokens, new IssueTokenValidator());
        tokens.Issue(Arg.Any<AuthenticatedUser>()).Returns(new TokenDto("jwt", DateTimeOffset.UtcNow.AddHours(1)));
    }

    private void Stored(User user) =>
        users.FindByUsernameAsync(user.Username, Arg.Any<CancellationToken>()).Returns(user);

    [Fact]
    public async Task Execute_ValidCredentials_IssuesTokenWithRoleAndCustomer()
    {
        var user = User.Create("maria", "hash", UserRole.Customer, "customer-7");
        Stored(user);
        hasher.Verify("hash", "secret").Returns(true);

        var token = await useCase.ExecuteAsync(new IssueTokenCommand("maria", "secret"), CancellationToken.None);

        Assert.Equal("jwt", token.AccessToken);
        tokens.Received(1).Issue(Arg.Is<AuthenticatedUser>(u =>
            u.Username == "maria" && u.Role == "Customer" && u.CustomerId == "customer-7"));
    }

    [Fact]
    public async Task Execute_WrongPassword_ThrowsInvalidCredentials()
    {
        var user = User.Create("maria", "hash", UserRole.Customer, "customer-7");
        Stored(user);
        hasher.Verify("hash", "wrong").Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            useCase.ExecuteAsync(new IssueTokenCommand("maria", "wrong"), CancellationToken.None));
        tokens.DidNotReceive().Issue(Arg.Any<AuthenticatedUser>());
    }

    [Fact]
    public async Task Execute_UnknownUser_ThrowsInvalidCredentialsAfterHashing()
    {
        users.FindByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            useCase.ExecuteAsync(new IssueTokenCommand("ghost", "secret"), CancellationToken.None));
        hasher.Received(1).Verify(string.Empty, "secret");
    }

    [Fact]
    public async Task Execute_BlankFields_FailsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecuteAsync(new IssueTokenCommand("", ""), CancellationToken.None));
    }
}
