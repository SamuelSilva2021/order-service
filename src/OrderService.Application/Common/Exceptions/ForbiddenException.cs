namespace OrderService.Application.Common.Exceptions;

public sealed class ForbiddenException(string message) : Exception(message);
