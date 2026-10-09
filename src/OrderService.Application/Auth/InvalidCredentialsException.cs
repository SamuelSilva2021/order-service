namespace OrderService.Application.Auth;

public sealed class InvalidCredentialsException() : Exception("Usuário ou senha inválidos.");
