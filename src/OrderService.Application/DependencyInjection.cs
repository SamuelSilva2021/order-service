using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Auth.Commands.IssueToken;
using OrderService.Application.Orders.Commands.CancelOrder;
using OrderService.Application.Orders.Commands.ConfirmOrder;
using OrderService.Application.Orders.Commands.CreateOrder;
using OrderService.Application.Orders.Common;
using OrderService.Application.Orders.Queries.GetOrder;
using OrderService.Application.Orders.Queries.ListOrders;
using OrderService.Application.Products.Commands.CreateProduct;
using OrderService.Application.Products.Queries.ListProducts;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>(lifetime: ServiceLifetime.Singleton);

        services.AddScoped<OrderStockService>();
        services.AddScoped<OrderAccessPolicy>();
        services.AddScoped<CreateOrderCommandHandler>();
        services.AddScoped<ConfirmOrderCommandHandler>();
        services.AddScoped<CancelOrderCommandHandler>();
        services.AddScoped<GetOrderQueryHandler>();
        services.AddScoped<ListOrdersQueryHandler>();
        services.AddScoped<CreateProductCommandHandler>();
        services.AddScoped<ListProductsQueryHandler>();
        services.AddScoped<IssueTokenCommandHandler>();

        return services;
    }
}
