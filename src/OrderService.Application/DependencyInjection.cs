using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Auth;
using OrderService.Application.Orders;
using OrderService.Application.Products;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>(lifetime: ServiceLifetime.Singleton);

        services.AddScoped<OrderStockService>();
        services.AddScoped<OrderAccessPolicy>();
        services.AddScoped<CreateOrderUseCase>();
        services.AddScoped<ConfirmOrderUseCase>();
        services.AddScoped<CancelOrderUseCase>();
        services.AddScoped<GetOrderUseCase>();
        services.AddScoped<ListOrdersUseCase>();
        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<ListProductsUseCase>();
        services.AddScoped<IssueTokenUseCase>();

        return services;
    }
}
