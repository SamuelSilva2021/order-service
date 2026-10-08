using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Abstractions;
using OrderService.Infrastructure.Auth;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Queries;
using OrderService.Infrastructure.Persistence.Repositories;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((provider, options) =>
            options.UseNpgsql(provider.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderQueries, OrderQueries>();
        services.AddScoped<IProductQueries, ProductQueries>();

        services.AddSingleton(TimeProvider.System);

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ConfiguredUsersOptions>().BindConfiguration(ConfiguredUsersOptions.SectionName);

        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        services.AddSingleton<IUserStore, ConfiguredUserStore>();

        return services;
    }
}
