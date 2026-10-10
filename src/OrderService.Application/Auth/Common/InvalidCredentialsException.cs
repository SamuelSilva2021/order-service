namespace OrderService.Application.Auth.Common;

public sealed class InvalidCredentialsException() : Exception("Usuário ou senha inválidos.");
