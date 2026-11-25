# Caché de respuestas con invalidación por etiqueta en ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Las solicitudes repetidas al catálogo pueden reutilizar una respuesta HTTP sin ejecutar el endpoint. Un cambio de precio debe actualizar las vistas del catálogo que lo muestran. En el laboratorio, una política con etiqueta define ese grupo y una revisión en la clave impide que una lectura nueva recupere datos de una revisión anterior.

## Identifica qué cambia la representación

La [política `catalog`](../src/CatalogCache.Api/Program.cs) usa `SetVaryByQuery("view")` porque `view` cambia el cuerpo de `GET /catalog`. Las variantes `brief` y `full` tienen entradas distintas. La ruta `/catalog/summary` tiene otro camino y usa la misma política. El middleware guarda las respuestas elegibles, como las respuestas públicas `200` de este ejemplo. Un `400` para una variante inválida queda fuera de la caché.

El contador `generation` aumenta cuando el endpoint produce una respuesta. Dos lecturas de la misma clave suelen devolver la misma generación hasta que la entrada expire o se invalide. Las respuestas con claves diferentes pueden tener distintas generaciones aunque usen la misma revisión del catálogo.

| Solicitud | Dimensiones observadas | Etiqueta |
| --- | --- | --- |
| `GET /catalog?view=brief` | Ruta, `brief`, revisión actual | `catalog` |
| `GET /catalog?view=full` | Ruta, `full`, revisión actual | `catalog` |
| `GET /catalog/summary` | Ruta y revisión actual | `catalog` |
| `GET /about` | Ruta independiente | `about` |

## Evita reutilizar datos de una revisión anterior

La actualización publica un nuevo [producto inmutable](../src/CatalogCache.Api/Catalog.cs) con una revisión mayor. Antes de consultar la caché, cada solicitud captura la referencia a la revisión visible en ese momento. El middleware usa esa revisión en `VaryByValue` y el endpoint usa el mismo producto para construir el cuerpo. Reutilizar la referencia impide guardar un cuerpo antiguo con una clave nueva.

La ruta de escritura llama a `EvictByTagAsync("catalog", ...)` para descartar las representaciones relacionadas. Una lectura iniciada antes del cambio aún puede responder con la revisión que capturó. Si llena la caché después de la invalidación, escribe con la clave anterior y no afecta a una lectura posterior con la revisión actual. El TTL de dos minutos limita la permanencia de las entradas anteriores.

## Verifica la invalidación selectiva

En el [inicio rápido](../README.es.md), haz dos lecturas iguales, consulta las otras variantes y cambia el precio. Comprueba el nuevo `priceInCents` y la nueva `revision` en las respuestas del catálogo. Compara el `generation` de `/about` antes y después. El cambio de precio conserva ese contenido.

Las [pruebas](../../tests/CatalogCache.Tests/CacheContracts.cs) ejercitan la invalidación por etiqueta con la revisión fija y también cambian la revisión sin invalidar la etiqueta. Estos casos comprueban ambas garantías de forma independiente.

## Elige el almacenamiento según el despliegue

El catálogo y la caché del laboratorio viven en el proceso local. Con varias instancias, la revisión necesita una fuente coherente entre réplicas. El almacenamiento de caché compartido y la política de expiración deben seguir ese contrato. Usa esta caché para respuestas que puedan compartir la misma clave. Las respuestas personalizadas necesitan una política que respete el alcance y los datos de cada usuario.

[Documentación de la caché de respuestas en ASP.NET Core](https://learn.microsoft.com/es-es/aspnet/core/performance/caching/output?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
