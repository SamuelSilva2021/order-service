namespace OrderService.Application.Products.Queries.ListProducts;

public sealed record ListProductsQuery(int Page = 1, int PageSize = 20);
