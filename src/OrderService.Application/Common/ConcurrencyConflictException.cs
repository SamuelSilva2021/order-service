namespace OrderService.Application.Common;

public sealed class ConcurrencyConflictException(Exception inner)
    : Exception("The resource was modified by another request. Retry the operation.", inner);
