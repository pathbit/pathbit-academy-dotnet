# Contratos de error HTTP con Problem Details

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Una aplicación cliente debe decidir el siguiente paso cuando falla una reserva. Puede pedir la corrección de un campo, avisar de que el producto ya no existe o sugerir una cantidad menor. La API del laboratorio devuelve estos resultados con el mismo formato JSON y un identificador estable para cada problema. El cliente puede tomar una decisión sin analizar el texto dirigido a personas.

## Diseña primero el contrato

El estado HTTP describe el tipo general de resultado. El campo `type` identifica un problema concreto dentro de esa clase. Usa `title` y, cuando esté presente, `detail` para mostrar información o diagnosticar el problema. Los campos adicionales también forman parte del contrato cuando los clientes dependen de ellos.

| Entrada en la previsualización | Estado | Tipo o resultado |
| --- | --- | --- |
| `keyboard`, cantidad 2 | `200` | Previsualización JSON con stock disponible |
| SKU vacío o cantidad fuera de 1–5 | `400` | `urn:pathbit:problem:invalid-reservation` y `errors` por campo |
| `mouse`, cantidad 1 | `404` | `urn:pathbit:problem:product-not-found` |
| `keyboard`, cantidad 3 | `409` | `urn:pathbit:problem:insufficient-stock` y `availableQuantity` |

El laboratorio solo tiene el SKU `keyboard`, con dos unidades disponibles. La previsualización consulta ese estado sin registrar una reserva. Una solicitud de tres unidades cumple el esquema, pero entra en conflicto con el stock actual. El cliente puede sugerir dos unidades al leer `availableQuantity`.

## Distingue tres orígenes de error

El [endpoint](../src/ProblemDetails.Api/Program.cs) usa `Results.ValidationProblem` para errores por campo y `Results.Problem` para resultados de negocio previstos. `AddProblemDetails` registra el servicio de escritura y añade `traceId` a la respuesta. Dos componentes del middleware intervienen antes de ejecutar las rutas.

- `UseExceptionHandler` convierte una excepción inesperada en `500` y conserva el diagnóstico técnico en los registros.
- `UseStatusCodePages` puede completar las respuestas de error sin cuerpo, como el `404` de una ruta inexistente. Se conserva el cuerpo que el endpoint ya haya generado.

El ejemplo usa `application/problem+json` para las respuestas negociadas como JSON. Si el cliente limita `Accept` a un formato sin escritor, el middleware puede usar una respuesta alternativa. Las integraciones deben declarar un tipo de medio compatible y comprobarlo antes de analizar el cuerpo.

## Correlaciona las respuestas con los registros internos

Consulta `/operations/fail` para observar un `500` con `traceId`. El identificador ayuda a relacionar la respuesta con la actividad en el servidor. El cuerpo omite el tipo y el mensaje de la excepción. La ruta reproduce un fallo en el laboratorio local.

Mantén estables los identificadores `urn:pathbit:problem:...` de los errores previstos. Un cambio de idioma o del texto mostrado puede conservar el mismo tipo. Añade extensiones con nombres y valores documentados, y guarda los datos privados de diagnóstico en los registros del servidor.

## Reproduce y verifica

Sigue el [inicio rápido](../README.es.md) y compara las respuestas con la tabla. Las [pruebas de contrato](../../tests/ProblemDetails.Tests/ProblemContracts.cs) comprueban el tipo de medio, los estados HTTP, los errores por campo, la correlación y la respuesta ante una excepción. También ejercitan el JSON mal formado y una ruta inexistente.

[Especificación RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) · [Control de errores en las API de ASP.NET Core](https://learn.microsoft.com/es-es/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
