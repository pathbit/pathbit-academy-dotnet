# HTTP error contracts with Problem Details

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

A client application needs to choose a next action when a reservation fails. It might ask for a field to be fixed, report that the product disappeared, or suggest a smaller quantity. The lab API returns these outcomes with one JSON format and a stable identifier for each problem. The client can decide what to do without parsing text meant for people.

## Model the contract first

The HTTP status describes the class of result. The `type` member identifies a specific problem within that class. Use `title` and, where present, `detail` for display and human diagnosis. Extra members are also part of the contract when clients depend on them.

| Preview input | Status | Type or outcome |
| --- | --- | --- |
| `keyboard`, quantity 2 | `200` | JSON preview with available stock |
| Empty SKU or quantity outside 1–5 | `400` | `urn:pathbit:problem:invalid-reservation` and field-level `errors` |
| `mouse`, quantity 1 | `404` | `urn:pathbit:problem:product-not-found` |
| `keyboard`, quantity 3 | `409` | `urn:pathbit:problem:insufficient-stock` and `availableQuantity` |

The lab has only the `keyboard` SKU, with two units available. The preview consults that state without writing a reservation. A request for three units passes input validation but conflicts with available stock. The client can suggest two units after reading `availableQuantity`.

## Distinguish three sources of errors

The [endpoint](../src/ProblemDetails.Api/Program.cs) uses `Results.ValidationProblem` for field errors and `Results.Problem` for expected domain outcomes. `AddProblemDetails` registers the writer service and adds `traceId` to the response. Two pieces of middleware run before routing handles the endpoint.

- `UseExceptionHandler` turns an unexpected exception into `500` and keeps the technical diagnosis in the logs.
- `UseStatusCodePages` can fill in bodyless error responses, such as `404` for an unknown route. A body already supplied by the endpoint is retained.

The example uses `application/problem+json` for responses negotiated as JSON. If a client limits its `Accept` header to a format with no registered writer, the middleware can use a fallback response. Integrations should declare a compatible media type and check it before parsing the body.

## Correlate errors while keeping internal details in the logs

Call `/operations/fail` to observe a `500` with `traceId`. The identifier helps match the response with the server activity. The body omits the exception type and message. The route exists to reproduce a failure in the local lab.

Keep the `urn:pathbit:problem:...` identifiers stable for expected errors. Changes to language or display text can reuse the existing type. Add extensions with documented names and values, and keep private diagnostic data in the server logs.

## Reproduce and verify

Follow the [quick start](../README.en.md) and compare the responses with the table. The [contract tests](../../tests/ProblemDetails.Tests/ProblemContracts.cs) check the media type, HTTP codes, field errors, correlation, and exception response. They also exercise malformed JSON and an unknown route.

[RFC 9457 specification](https://www.rfc-editor.org/rfc/rfc9457) · [ASP.NET Core API error handling](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
