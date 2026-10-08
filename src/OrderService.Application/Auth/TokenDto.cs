namespace OrderService.Application.Auth;

public sealed record TokenDto(string AccessToken, DateTimeOffset ExpiresAt);
