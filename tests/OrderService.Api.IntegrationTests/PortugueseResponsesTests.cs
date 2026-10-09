using System.Net;
using System.Text;
using OrderService.Api.IntegrationTests.Support;

namespace OrderService.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class PortugueseResponsesTests(ApiFixture fixture)
{
    private static async Task<string> BodyOf(HttpResponseMessage response) => await response.Content.ReadAsStringAsync();

    private static StringContent Json(string content) => new(content, Encoding.UTF8, "application/json");

    [Fact]
    public async Task Validation_RuleViolation_IsInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.PlaceOrderAsync("customer-001");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await BodyOf(response);
        Assert.Contains("Um ou mais erros de validação ocorreram.", body);
        Assert.Contains("O pedido deve ter ao menos um item.", body);
    }

    [Fact]
    public async Task Validation_MissingFields_AreReportedInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.Http.PostAsync("/orders", Json("{}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await BodyOf(response);
        Assert.Contains("O customerId é obrigatório.", body);
        Assert.Contains("A moeda é obrigatória.", body);
        Assert.Contains("O pedido deve ter ao menos um item.", body);
    }

    [Fact]
    public async Task MalformedJson_IsReportedInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.Http.PostAsync("/orders", Json("{ invalid"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await BodyOf(response);
        Assert.Contains("Um ou mais erros de validação ocorreram.", body);
        Assert.Contains("O corpo da requisição é inválido.", body);
    }

    [Fact]
    public async Task InvalidGuidInBody_IsReportedInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();
        var payload = """{"customerId":"c","currency":"BRL","items":[{"productId":"abc","quantity":1}]}""";

        var response = await admin.Http.PostAsync("/orders", Json(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("O corpo da requisição é inválido.", await BodyOf(response));
    }

    [Fact]
    public async Task InvalidQueryValue_IsReportedInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.ListOrdersAsync("status=Unknown");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("não é válido", await BodyOf(response));
    }

    [Fact]
    public async Task NotFound_IsInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();

        var response = await admin.GetOrderAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await BodyOf(response);
        Assert.Contains("Recurso não encontrado.", body);
        Assert.Contains("não foi encontrado.", body);
    }

    [Fact]
    public async Task InsufficientStock_IsInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 1);

        var response = await admin.PlaceOrderAsync("customer-001", (product.Id, 2));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await BodyOf(response);
        Assert.Contains("Conflito.", body);
        Assert.Contains("unidade(s) disponível(is)", body);
    }

    [Fact]
    public async Task InvalidTransition_IsInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var order = await admin.PlaceOrderOkAsync("customer-001", (product.Id, 1));
        await admin.CancelAsync(order.Id);

        var response = await admin.ConfirmAsync(order.Id);

        var body = await BodyOf(response);
        Assert.Contains("não pode passar de Cancelado para Confirmado.", body);
        Assert.DoesNotContain("Canceled", body);
    }

    [Fact]
    public async Task Forbidden_IsInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();
        var customer = await fixture.CreateCustomerAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);

        var response = await customer.PlaceOrderAsync("customer-002", (product.Id, 1));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await BodyOf(response);
        Assert.Contains("Acesso negado.", body);
        Assert.Contains("Você só pode criar pedidos para o seu próprio cliente.", body);
    }

    [Fact]
    public async Task InvalidCredentials_AreInPortuguese()
    {
        var response = await fixture.CreateAnonymous().RequestTokenAsync("admin", "wrong");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await BodyOf(response);
        Assert.Contains("Não autorizado.", body);
        Assert.Contains("Usuário ou senha inválidos.", body);
    }

    [Fact]
    public async Task BusinessRule_CurrencyMismatch_IsInPortuguese()
    {
        var admin = await fixture.CreateAdminAsync();
        var product = await admin.CreateProductAsync(availableQuantity: 5);
        var payload = $$"""{"customerId":"c","currency":"USD","items":[{"productId":"{{product.Id}}","quantity":1}]}""";

        var response = await admin.Http.PostAsync("/orders", Json(payload));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await BodyOf(response);
        Assert.Contains("Regra de negócio violada.", body);
        Assert.Contains("é precificado em", body);
    }
}
