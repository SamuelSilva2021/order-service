namespace OrderService.Application.Auth;

public sealed class InvalidCredentialsException() : Exception("Invalid username or password.");
