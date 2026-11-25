# Caché de respuestas con invalidación por etiqueta en ASP.NET Core

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

Una API de catálogo reutiliza respuestas HTTP y actualiza solo las representaciones afectadas por un cambio de precio. El laboratorio usa la caché en memoria de ASP.NET Core y funciona sin credenciales.

[Lee el artículo](article/ARTICLE.es.md) · [Catálogo .NET](../README.es.md)

## Requisitos previos

- [SDK de .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- [curl para ejecutar los comandos HTTP en una terminal](https://curl.se/download.html).

## Inicio rápido

Inicia la API en loopback desde el directorio del laboratorio.

```bash
dotnet run --project src/CatalogCache.Api --no-launch-profile -- --urls http://127.0.0.1:19806
```

## Observa la reutilización y la invalidación

Envía las solicitudes desde otra terminal. Las dos primeras respuestas comparten el mismo `generation`. La variante `full` y la ruta de resumen tienen sus propias generaciones. Tras el `POST`, las rutas del catálogo devuelven `priceInCents: 12900`, una nueva `revision` y nuevos valores de `generation`. La respuesta de `/about` conserva su generación anterior.

```bash
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=brief'
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=brief'
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=full'
curl -sS -w '\n' http://127.0.0.1:19806/catalog/summary
curl -sS -w '\n' http://127.0.0.1:19806/about
curl -sS -w '\n' http://127.0.0.1:19806/catalog/price \
  -H 'Content-Type: application/json' -d '{"priceInCents":12900}'
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=brief'
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=full'
curl -sS -w '\n' http://127.0.0.1:19806/catalog/summary
curl -sS -w '\n' http://127.0.0.1:19806/about
```

Pulsa `Ctrl+C` para detener la API.

## Ejecuta las pruebas

Las pruebas comprueban las variantes de consulta, la invalidación selectiva y la separación de las revisiones sin esperar a que expire el TTL.

```bash
dotnet test ../tests/CatalogCache.Tests
```

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
