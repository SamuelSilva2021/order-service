using System.Text.Json.Serialization;
using OrderService.Api.Authentication;
using OrderService.Api.ErrorHandling;
using OrderService.Api.Swagger;
using OrderService.Application;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddJwtAuthentication()
    .AddSwaggerWithJwt()
    .AddProblemDetails()
    .AddPortugueseMessages()
    .AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
