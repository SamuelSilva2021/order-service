namespace OrderService.Application.Common;

public sealed class ConcurrencyConflictException(Exception inner)
    : Exception("O recurso foi modificado por outra requisição. Tente novamente.", inner);
