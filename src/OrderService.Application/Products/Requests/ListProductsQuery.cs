namespace OrderService.Application.Products.Requests;

public sealed record ListProductsQuery(int Page = 1, int PageSize = 20);
