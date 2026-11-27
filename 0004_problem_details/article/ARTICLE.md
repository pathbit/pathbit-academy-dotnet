# Como devolver erros que um cliente consegue tratar com Problem Details

[English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Na tela de reserva, o produto ainda existe e o usuário pede três teclados. A API só tem dois. Se o cliente recebe apenas «algo deu errado», ele não consegue saber se vale corrigir o formulário ou esperar o estoque. Um contrato Problem Details permite distinguir um conflito de estoque de erro de validação e de falha inesperada.

## Use o código HTTP para a classe e `type` para a decisão

| Requisição à prévia | HTTP | O que o cliente pode fazer |
| --- | --- | --- |
| `keyboard` com quantidade 2 | `200` | Exibir a prévia |
| SKU vazio ou quantidade fora de 1 a 5 | `400` | Ler `errors` e marcar os campos |
| `mouse` com quantidade 1 | `404` | Informar produto indisponível |
| `keyboard` com quantidade 3 | `409` | Ler `availableQuantity` e oferecer até duas unidades |

Na resposta de conflito, o identificador é `urn:pathbit:problem:insufficient-stock`. A equipe pode traduzir `title` e `detail` sem mudar o campo que orienta a interface. Os dados do formulário seguem nas extensões `errors` e `availableQuantity` quando aplicáveis. O contrato vale para uma prévia. Ele não cria nem congela uma reserva.

## Monte o pipeline em torno do mesmo formato

O [endpoint](../src/ProblemDetails.Api/Program.cs) usa `Results.ValidationProblem` para erros por campo e `Results.Problem` para resultados de negócio. `AddProblemDetails` registra o escritor e personaliza `traceId`. Coloque `UseExceptionHandler` e `UseStatusCodePages` antes das rotas. O primeiro trata exceções inesperadas. O segundo pode completar respostas vazias como um `404` de rota inexistente.

```csharp
app.UseExceptionHandler();
app.UseStatusCodePages();
```

O JSON de problemas usa `application/problem+json`. Em respostas produzidas por middleware, a negociação depende do `Accept` e dos escritores registrados. Confira o cabeçalho de resposta antes de analisar o corpo. Para uma falha inesperada, investigue o `traceId` no servidor e mantenha a mensagem da exceção nos logs.

```csharp
if (request.Quantity > availableQuantity)
{
    return Results.Problem(
        statusCode: StatusCodes.Status409Conflict,
        type: "urn:pathbit:problem:insufficient-stock",
        title: "The requested quantity exceeds available stock.",
        extensions: new Dictionary<string, object?> { ["availableQuantity"] = availableQuantity });
}
```

## Faça a leitura como um consumidor da API

Execute as chamadas do [início rápido](../README.md). Compare primeiro os dois casos de `400` e `409`. Depois chame `/operations/fail` e verifique que a rota devolve `500` com correlação. Os [testes de contrato](../../tests/ProblemDetails.Tests/ProblemContracts.cs) também exercitam JSON malformado e rota inexistente.

Se estiver integrando uma API versionada, trate identificadores e campos usados pelo cliente como parte do [contrato de versão](../../0002_versionamento_api/article/ARTICLE.md). O [próximo laboratório](../../0005_filas_limitadas/article/ARTICLE.md) dá outro exemplo de decisão do consumidor, a resposta de uma fila temporariamente cheia.

[RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) · [Tratamento de erros em APIs ASP.NET Core](https://learn.microsoft.com/pt-br/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0)

---

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
