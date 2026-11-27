# Reutiliza las respuestas de catálogo con Output Caching en ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Una tienda ofrece vistas breve y completa del catálogo. Ambas deben mostrar el nuevo precio después de una edición, mientras las páginas independientes conservan su caché. El laboratorio hace visibles la reutilización, la clave y la invalidación.

## Diseña el comportamiento

La política del catálogo varía por `view` y por la revisión del catálogo. `brief`, `full` y `/catalog/summary` comparten la etiqueta `catalog`. `/about` tiene su propia etiqueta. `generation` cambia cuando el controlador crea una respuesta. `revision` identifica el estado del producto utilizado.

## Lee la implementación

Captura el mismo producto inmutable para la clave y el cuerpo. La escritura invalida la etiqueta `catalog`. Una lectura iniciada antes de la escritura sigue usando la clave anterior aunque termine después. Las lecturas posteriores usan la nueva revisión y el TTL limita la vida de las entradas antiguas.

[Código fuente](../src/CatalogCache.Api/Program.cs)

## Ejecuta el laboratorio

Sigue la secuencia del ejercicio. Lee `brief` dos veces, consulta `full` y el resumen, y cambia el precio a `12900` centavos. Comprueba la nueva revisión en el catálogo y la generación estable de `/about` dentro del TTL. Con réplicas, usa una fuente coherente de revisiones y un almacenamiento compatible con tu política.

[Guía completa](../README.es.md) · [Documentación oficial](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
