using OrderService.Domain.Common;

namespace OrderService.Domain.Products;

public sealed class InsufficientStockException(Guid productId, int requested, int available)
    : DomainException($"O produto {productId} possui {available} unidade(s) disponível(is), mas foram solicitadas {requested}.");
