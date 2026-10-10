namespace OrderService.Application.Common.Interfaces;

public interface ICurrentUser
{
    bool IsAdmin { get; }

    string? CustomerId { get; }
}
