namespace OrderService.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAdmin { get; }

    string? CustomerId { get; }
}
