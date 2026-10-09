namespace OrderService.Application.Auth.Dtos;

public sealed record TokenDto(string AccessToken, DateTimeOffset ExpiresAt);
