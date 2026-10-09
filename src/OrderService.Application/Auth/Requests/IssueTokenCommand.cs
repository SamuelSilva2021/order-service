namespace OrderService.Application.Auth.Requests;

public sealed record IssueTokenCommand(string Username, string Password);
