using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace OrderService.Api.IntegrationTests.Support;

public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string CustomerOneId = "customer-001";
    public const string CustomerTwoId = "customer-002";

    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private readonly ConcurrentDictionary<string, Lazy<Task<string>>> tokens = new();

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

    public Task<ApiClient> CreateSecondCustomerAsync() => CreateClientAsync("customer2", "customer123");

    public ApiClient CreateAnonymous() => new(CreateClient());

    private async Task<ApiClient> CreateClientAsync(string username, string password)
    {
        var token = await tokens.GetOrAdd(username, _ => new Lazy<Task<string>>(() => RequestTokenAsync(username, password))).Value;
        var client = new ApiClient(CreateClient());
        client.UseToken(token);
        return client;
    }

    private async Task<string> RequestTokenAsync(string username, string password)
    {
        var client = new ApiClient(CreateClient());
        await client.SignInAsync(username, password);
        return client.AccessToken!;
    }
}
