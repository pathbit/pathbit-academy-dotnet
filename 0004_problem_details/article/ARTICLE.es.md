# Usa Problem Details para devolver errores HTTP que el cliente pueda tratar

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Una persona pide tres teclados. El producto existe, pero la tienda tiene dos. El cliente necesita distinguir un conflicto de stock de un producto ausente y de una entrada inválida para ofrecer una acción adecuada.

## Diseña el comportamiento

Devuelve `400` con `errors` por campo para una entrada inválida. Devuelve `404` para un SKU desconocido y `409` con `availableQuantity` para un conflicto. Usa un `type` estable como `urn:pathbit:problem:insufficient-stock` para que el cliente pueda decidir sin leer un texto traducido.

## Lee la implementación

Examina `Results.ValidationProblem`, `Results.Problem` y `AddProblemDetails`. El gestor de excepciones genera un `500` genérico con `traceId`. Las páginas de estado pueden completar errores sin cuerpo. Conserva los detalles de excepción en el servidor y revisa el `Content-Type` negociado antes de decodificar.

[Código fuente](../src/ProblemDetails.Api/Program.cs)

## Ejecuta el laboratorio

Ejecuta la previsualización con `keyboard` y cantidad `2`, luego `3`, y llama a `/operations/fail`. Compara `200`, `409` y `500`. Las pruebas cubren también errores por campo, JSON malformado y rutas desconocidas.

[Guía completa](../README.es.md) · [Documentación oficial](https://www.rfc-editor.org/rfc/rfc9457)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
