namespace OrderService.Application.Auth.Commands.IssueToken;

public sealed record IssueTokenCommand(string Username, string Password);
