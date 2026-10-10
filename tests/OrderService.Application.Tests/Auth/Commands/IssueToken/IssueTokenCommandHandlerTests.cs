using FluentValidation;
using NSubstitute;
using OrderService.Application.Auth.Commands.IssueToken;
using OrderService.Application.Auth.Common;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Users;

namespace OrderService.Application.Tests.Auth.Commands.IssueToken;

public class IssueTokenCommandHandlerTests
{
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenIssuer tokens = Substitute.For<ITokenIssuer>();
    private readonly IssueTokenCommandHandler handler;

    public IssueTokenCommandHandlerTests()
    {
        handler = new IssueTokenCommandHandler(users, hasher, tokens, new IssueTokenCommandValidator());
        tokens.Issue(Arg.Any<AuthenticatedUser>()).Returns(call => new TokenDto("jwt", DateTimeOffset.UtcNow.AddHours(1), call.Arg<AuthenticatedUser>()));
    }

    private void Stored(User user) =>
        users.FindByUsernameAsync(user.Username, Arg.Any<CancellationToken>()).Returns(user);

    [Fact]
    public async Task Execute_ValidCredentials_IssuesTokenWithRoleAndCustomer()
    {
        var user = User.Create("maria", "hash", UserRole.Customer, "customer-7");
        Stored(user);
        hasher.Verify("hash", "secret").Returns(true);

        var token = await handler.HandleAsync(new IssueTokenCommand("maria", "secret"), CancellationToken.None);

        Assert.Equal("jwt", token.AccessToken);
        Assert.Equal("customer-7", token.User.CustomerId);
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
            handler.HandleAsync(new IssueTokenCommand("maria", "wrong"), CancellationToken.None));
        tokens.DidNotReceive().Issue(Arg.Any<AuthenticatedUser>());
    }

    [Fact]
    public async Task Execute_UnknownUser_ThrowsInvalidCredentialsAfterHashing()
    {
        users.FindByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(new IssueTokenCommand("ghost", "secret"), CancellationToken.None));
        hasher.Received(1).Verify(string.Empty, "secret");
    }

    [Fact]
    public async Task Execute_BlankFields_FailsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(new IssueTokenCommand("", ""), CancellationToken.None));
    }
}
