# Como o cliente descobre que faltou estoque?

A prévia de reserva devolve `409` com um tipo estável e `availableQuantity`. A interface usa esses campos para propor uma quantidade menor. Para um formulário inválido, recebe `400` com campos a corrigir.

## Use o código HTTP para a classe e `type` para a decisão

| Requisição à prévia | HTTP | O que o cliente pode fazer |
| --- | --- | --- |
| `keyboard` com quantidade 2 | `200` | Exibir a prévia |
| SKU vazio ou quantidade fora de 1 a 5 | `400` | Ler `errors` e marcar os campos |
| `mouse` com quantidade 1 | `404` | Informar produto indisponível |
| `keyboard` com quantidade 3 | `409` | Ler `availableQuantity` e oferecer até duas unidades |

Na resposta de conflito, o identificador é `urn:pathbit:problem:insufficient-stock`. A equipe pode traduzir `title` e `detail` sem mudar o campo que orienta a interface. Os dados do formulário seguem nas extensões `errors` e `availableQuantity` quando aplicáveis. O contrato vale para uma prévia. Ele não cria nem congela uma reserva.

## Monte o pipeline em torno do mesmo formato

O [endpoint](https://github.com/pathbit/pathbit-academy-dotnet/blob/master/0004_problem_details/src/ProblemDetails.Api/Program.cs) usa `Results.ValidationProblem` para erros por campo e `Results.Problem` para resultados de negócio. `AddProblemDetails` registra o escritor e personaliza `traceId`. Coloque `UseExceptionHandler` e `UseStatusCodePages` antes das rotas. O primeiro trata exceções inesperadas. O segundo pode completar respostas vazias como um `404` de rota inexistente.

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

## Reproduza o cenário no laboratório

Rode os dois cenários com o mesmo endpoint e faça uma chamada a `/operations/fail`. Compare o tipo de erro, o `Content-Type` e o `traceId`. O texto exibido pode ser localizado sem alterar o contrato de decisão.

[Artigo completo e laboratório no GitHub](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0004_problem_details)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
