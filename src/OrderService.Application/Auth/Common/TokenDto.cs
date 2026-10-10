namespace OrderService.Application.Auth.Common;

public sealed record TokenDto(string AccessToken, DateTimeOffset ExpiresAt, AuthenticatedUser User);
