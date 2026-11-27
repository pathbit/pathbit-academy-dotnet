# Reutiliza las respuestas de catálogo con Output Caching en ASP.NET Core

Una tienda ofrece vistas breve y completa del catálogo. Ambas deben mostrar el nuevo precio después de una edición, mientras las páginas independientes conservan su caché. El laboratorio hace visibles la reutilización, la clave y la invalidación.

## Diseña el comportamiento

La política del catálogo varía por `view` y por la revisión del catálogo. `brief`, `full` y `/catalog/summary` comparten la etiqueta `catalog`. `/about` tiene su propia etiqueta. `generation` cambia cuando el controlador crea una respuesta. `revision` identifica el estado del producto utilizado.

## Ejecuta el laboratorio

Sigue la secuencia del ejercicio. Lee `brief` dos veces, consulta `full` y el resumen, y cambia el precio a `12900` centavos. Comprueba la nueva revisión en el catálogo y la generación estable de `/about` dentro del TTL. Con réplicas, usa una fuente coherente de revisiones y un almacenamiento compatible con tu política.

[Guía completa](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0006_output_cache/article/ARTICLE.es.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
