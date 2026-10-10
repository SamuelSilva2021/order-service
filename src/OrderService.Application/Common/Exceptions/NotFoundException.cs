namespace OrderService.Application.Common.Exceptions;

public sealed class NotFoundException(string resource, Guid id)
    : Exception($"{resource} {id} não foi encontrado.");
