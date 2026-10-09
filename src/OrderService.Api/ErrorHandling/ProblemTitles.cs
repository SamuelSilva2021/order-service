namespace OrderService.Api.ErrorHandling;

public static class ProblemTitles
{
    public const string Validation = "Um ou mais erros de validação ocorreram.";
    public const string NotFound = "Recurso não encontrado.";
    public const string Unauthorized = "Não autorizado.";
    public const string Forbidden = "Acesso negado.";
    public const string Conflict = "Conflito.";
    public const string BusinessRule = "Regra de negócio violada.";
    public const string Unexpected = "Ocorreu um erro inesperado.";
    public const string MethodNotAllowed = "Método não permitido.";
    public const string NotAcceptable = "Formato de resposta não aceito.";
    public const string UnsupportedMediaType = "Tipo de conteúdo não suportado.";
}
