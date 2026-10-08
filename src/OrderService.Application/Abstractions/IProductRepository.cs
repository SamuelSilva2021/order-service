using OrderService.Domain.Products;

namespace OrderService.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    void Add(Product product);
}
