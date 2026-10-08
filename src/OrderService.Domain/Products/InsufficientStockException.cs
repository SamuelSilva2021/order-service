using OrderService.Domain.Common;

namespace OrderService.Domain.Products;

public sealed class InsufficientStockException(Guid productId, int requested, int available)
    : DomainException($"Product {productId} has {available} unit(s) available but {requested} were requested.");
