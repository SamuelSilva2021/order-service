namespace OrderService.Application.Auth;

public sealed record IssueTokenCommand(string Username, string Password);
