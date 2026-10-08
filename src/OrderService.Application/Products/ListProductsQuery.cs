namespace OrderService.Application.Products;

public sealed record ListProductsQuery(int Page = 1, int PageSize = 20);
