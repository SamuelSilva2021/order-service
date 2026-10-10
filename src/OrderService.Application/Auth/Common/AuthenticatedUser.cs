namespace OrderService.Application.Auth.Common;

public sealed record AuthenticatedUser(string Username, string Role, string? CustomerId);
