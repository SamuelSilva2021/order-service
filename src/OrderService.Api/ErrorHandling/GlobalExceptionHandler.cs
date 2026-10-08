using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Auth;
using OrderService.Application.Common;
using OrderService.Domain.Common;
using OrderService.Domain.Orders;
using OrderService.Domain.Products;

namespace OrderService.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var problem = Describe(exception);

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);
        }

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(
            problem,
            problem.GetType(),
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }

    private static ProblemDetails Describe(Exception exception) => exception switch
    {
        ValidationException validation => new ValidationProblemDetails(GroupErrors(validation))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        },
        NotFoundException => Problem(StatusCodes.Status404NotFound, "Resource not found.", exception),
        InvalidCredentialsException => Problem(StatusCodes.Status401Unauthorized, "Unauthorized.", exception),
        InsufficientStockException or InvalidOrderTransitionException or ConcurrencyConflictException =>
            Problem(StatusCodes.Status409Conflict, "Conflict.", exception),
        DomainException => Problem(StatusCodes.Status422UnprocessableEntity, "Business rule violated.", exception),
        _ => new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = "An unexpected error occurred." }
    };

    private static ProblemDetails Problem(int status, string title, Exception exception) =>
        new() { Status = status, Title = title, Detail = exception.Message };

    private static Dictionary<string, string[]> GroupErrors(ValidationException exception) =>
        exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
}
