using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace OrderService.Api.ErrorHandling;

public static class PortugueseMessagesExtensions
{
    private const string InvalidBody = "O corpo da requisição é inválido.";

    public static IServiceCollection AddPortugueseMessages(this IServiceCollection services)
    {
        services.PostConfigure<MvcOptions>(options =>
        {
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            TranslateModelBinding(options.ModelBindingMessageProvider);
        });

        services.PostConfigure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = CreateValidationResponse;
            TranslateClientErrors(options);
        });

        return services;
    }

    private static IActionResult CreateValidationResponse(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => IsJsonError(entry.Key, error) ? InvalidBody : error.ErrorMessage)
                    .ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ProblemTitles.Validation,
            Instance = context.HttpContext.Request.Path
        };

        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }

    private static bool IsJsonError(string key, ModelError error) =>
        error.Exception is not null || key.StartsWith("$", StringComparison.Ordinal);

    private static void TranslateClientErrors(ApiBehaviorOptions options)
    {
        var titles = new Dictionary<int, string>
        {
            [StatusCodes.Status401Unauthorized] = ProblemTitles.Unauthorized,
            [StatusCodes.Status403Forbidden] = ProblemTitles.Forbidden,
            [StatusCodes.Status404NotFound] = ProblemTitles.NotFound,
            [StatusCodes.Status405MethodNotAllowed] = ProblemTitles.MethodNotAllowed,
            [StatusCodes.Status406NotAcceptable] = ProblemTitles.NotAcceptable,
            [StatusCodes.Status409Conflict] = ProblemTitles.Conflict,
            [StatusCodes.Status415UnsupportedMediaType] = ProblemTitles.UnsupportedMediaType,
            [StatusCodes.Status422UnprocessableEntity] = ProblemTitles.BusinessRule,
            [StatusCodes.Status500InternalServerError] = ProblemTitles.Unexpected
        };

        foreach (var (status, title) in titles)
        {
            if (options.ClientErrorMapping.TryGetValue(status, out var data))
            {
                data.Title = title;
            }
        }
    }

    private static void TranslateModelBinding(DefaultModelBindingMessageProvider provider)
    {
        provider.SetValueMustNotBeNullAccessor(_ => "O valor é obrigatório.");
        provider.SetMissingBindRequiredValueAccessor(field => $"O campo '{field}' é obrigatório.");
        provider.SetMissingKeyOrValueAccessor(() => "É necessário informar a chave e o valor.");
        provider.SetMissingRequestBodyRequiredValueAccessor(() => "O corpo da requisição é obrigatório.");
        provider.SetAttemptedValueIsInvalidAccessor((value, field) => $"O valor '{value}' não é válido para '{field}'.");
        provider.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"O valor '{value}' não é válido.");
        provider.SetUnknownValueIsInvalidAccessor(field => $"O valor informado não é válido para '{field}'.");
        provider.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado não é válido.");
        provider.SetValueIsInvalidAccessor(value => $"O valor '{value}' não é válido.");
        provider.SetValueMustBeANumberAccessor(field => $"O campo '{field}' deve ser um número.");
        provider.SetNonPropertyValueMustBeANumberAccessor(() => "O valor deve ser um número.");
    }
}
