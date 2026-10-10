using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Products;

namespace OrderService.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await context.Products.Where(product => ids.Contains(product.Id)).ToListAsync(cancellationToken);

    public void Add(Product product) => context.Products.Add(product);
}
