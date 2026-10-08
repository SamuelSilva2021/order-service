using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace OrderService.Api.IntegrationTests.Support;

public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:Default", postgres.GetConnectionString());

    public async Task InitializeAsync() => await postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await postgres.DisposeAsync();
    }

    public Task<ApiClient> CreateAdminAsync() => CreateClientAsync("admin", "admin123");

    public Task<ApiClient> CreateCustomerAsync() => CreateClientAsync("customer", "customer123");

    public ApiClient CreateAnonymous() => new(CreateClient());

    private async Task<ApiClient> CreateClientAsync(string username, string password)
    {
        var client = new ApiClient(CreateClient());
        await client.SignInAsync(username, password);
        return client;
    }
}
