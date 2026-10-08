namespace OrderService.Application.Common;

public sealed class NotFoundException(string resource, Guid id)
    : Exception($"{resource} {id} was not found.");
