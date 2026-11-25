# Contratos de error HTTP con Problem Details

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

Una API de previsualización de reservas devuelve errores de validación, productos inexistentes y conflictos de stock en el formato de la RFC 9457. El laboratorio usa datos en memoria y funciona sin credenciales.

[Lee el artículo](article/ARTICLE.es.md) · [Catálogo .NET](../README.es.md)

## Requisitos previos

- [SDK de .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- [curl para ejecutar los comandos HTTP en una terminal](https://curl.se/download.html).

## Inicio rápido

Inicia la API en loopback desde el directorio del laboratorio.

```bash
dotnet run --project src/ProblemDetails.Api --no-launch-profile -- --urls http://127.0.0.1:19804
```

## Prueba el contrato

Envía las solicitudes desde otra terminal. La primera devuelve `200` con la previsualización. Las cuatro siguientes devuelven `400`, `404`, `409` y `500`. Observa `Content-Type`, `type`, `status`, `traceId` y los campos específicos de cada error.

```bash
curl -si -w '\n' http://127.0.0.1:19804/reservations/preview \
  -H 'Content-Type: application/json' -d '{"sku":"keyboard","quantity":2}'
curl -si -w '\n' http://127.0.0.1:19804/reservations/preview \
  -H 'Content-Type: application/json' -d '{"sku":"","quantity":0}'
curl -si -w '\n' http://127.0.0.1:19804/reservations/preview \
  -H 'Content-Type: application/json' -d '{"sku":"mouse","quantity":1}'
curl -si -w '\n' http://127.0.0.1:19804/reservations/preview \
  -H 'Content-Type: application/json' -d '{"sku":"keyboard","quantity":3}'
curl -si -w '\n' http://127.0.0.1:19804/operations/fail
```

Pulsa `Ctrl+C` para detener la API.

## Ejecuta las pruebas

Ejecuta el comando desde el directorio del laboratorio. Las pruebas levantan el servidor en memoria sin ocupar el puerto del ejemplo.

```bash
dotnet test ../tests/ProblemDetails.Tests
```

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
