# Usa Problem Details para devolver errores HTTP que el cliente pueda tratar

Una persona pide tres teclados. El producto existe, pero la tienda tiene dos. El cliente necesita distinguir un conflicto de stock de un producto ausente y de una entrada inválida para ofrecer una acción adecuada.

## Diseña el comportamiento

Devuelve `400` con `errors` por campo para una entrada inválida. Devuelve `404` para un SKU desconocido y `409` con `availableQuantity` para un conflicto. Usa un `type` estable como `urn:pathbit:problem:insufficient-stock` para que el cliente pueda decidir sin leer un texto traducido.

## Ejecuta el laboratorio

Ejecuta la previsualización con `keyboard` y cantidad `2`, luego `3`, y llama a `/operations/fail`. Compara `200`, `409` y `500`. Las pruebas cubren también errores por campo, JSON malformado y rutas desconocidas.

[Guía completa](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0004_problem_details/article/ARTICLE.es.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
