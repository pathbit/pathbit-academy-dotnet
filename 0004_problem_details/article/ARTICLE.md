# Contratos de erro HTTP com Problem Details

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Uma aplicação cliente precisa escolher a próxima ação quando uma reserva falha. Pode pedir a correção de um campo, avisar que o produto sumiu ou sugerir uma quantidade menor. A API do laboratório devolve esses resultados com o mesmo formato JSON e um identificador estável por problema. O cliente pode tomar a decisão sem analisar o texto destinado a pessoas.

## Modele o contrato antes de implementar

O status HTTP comunica a classe de resultado. O campo `type` identifica um problema específico dentro dessa classe. Use `title` e `detail`, quando houver, para exibição e diagnóstico humano. Os campos adicionais também fazem parte do contrato se o cliente depender deles.

| Entrada na prévia | Status | Tipo ou resultado |
| --- | --- | --- |
| `keyboard`, quantidade 2 | `200` | Prévia JSON com estoque disponível |
| SKU vazio ou quantidade fora de 1–5 | `400` | `urn:pathbit:problem:invalid-reservation` e `errors` por campo |
| `mouse`, quantidade 1 | `404` | `urn:pathbit:problem:product-not-found` |
| `keyboard`, quantidade 3 | `409` | `urn:pathbit:problem:insufficient-stock` e `availableQuantity` |

O laboratório tem somente o SKU `keyboard`, com duas unidades. A prévia consulta essa situação sem gravar uma reserva. Um pedido de três unidades é válido no esquema, mas conflita com o estoque atual. O cliente pode oferecer duas unidades ao ler `availableQuantity`.

## Separe as três origens dos erros

O [endpoint](../src/ProblemDetails.Api/Program.cs) usa `Results.ValidationProblem` para erros por campo e `Results.Problem` para resultados esperados de negócio. `AddProblemDetails` registra o serviço de escrita e acrescenta `traceId` à resposta. O pipeline usa dois middlewares antes das rotas.

- `UseExceptionHandler` converte uma exceção inesperada em `500` e deixa o diagnóstico técnico nos logs.
- `UseStatusCodePages` pode preencher respostas de erro sem corpo, como o `404` de uma rota inexistente. Um corpo já preenchido pelo endpoint é mantido.

O exemplo usa `application/problem+json` para as respostas negociadas como JSON. Se o cliente restringir o cabeçalho `Accept` a um formato sem escritor, os middlewares podem usar a resposta alternativa. Integrações devem declarar um tipo de mídia compatível e conferi-lo antes de processar o corpo.

## Correlacione sem expor dados internos

Chame `/operations/fail` para observar um `500` com `traceId`. O identificador ajuda a cruzar a resposta com a atividade registrada no servidor. O corpo não envia o tipo ou a mensagem da exceção. A rota existe para reproduzir a falha no laboratório local.

Para erros esperados, mantenha os identificadores `urn:pathbit:problem:...` estáveis. Uma mudança de idioma ou texto de apresentação não precisa mudar o tipo. Adicione extensões com nomes e valores documentados. Separe dados de diagnóstico interno dos campos que o consumidor realmente usa.

## Reproduza e verifique

Siga o [começo rápido](../README.md) e compare as respostas com a tabela. Os [testes de contrato](../../tests/ProblemDetails.Tests/ProblemContracts.cs) conferem o tipo de mídia, os códigos HTTP, os campos de validação, a correlação e a resposta de exceção. Eles também exercitam JSON malformado e uma rota que não existe.

[Especificação RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) · [Tratamento de erros em APIs ASP.NET Core](https://learn.microsoft.com/pt-br/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
