# HTTP error contracts with Problem Details

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

A reservation preview API returns validation errors, missing products, and stock conflicts in RFC 9457 format. The lab uses in-memory data and requires no credentials.

[Read the article](article/ARTICLE.en.md) · [.NET catalogue](../README.en.md)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- [curl to run the HTTP commands in a terminal](https://curl.se/download.html).

## Quick start

Start the API on loopback from this lab directory.

```bash
dotnet run --project src/ProblemDetails.Api --no-launch-profile -- --urls http://127.0.0.1:19804
```

## Try the contract

Send the requests in a second terminal. The first returns `200` with the preview. The next four return `400`, `404`, `409`, and `500`. Inspect `Content-Type`, `type`, `status`, `traceId`, and the fields specific to each error.

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

Press `Ctrl+C` to stop the API.

## Run the tests

Run this command from the lab directory. The tests use an in-memory server without occupying the example's port.

```bash
dotnet test ../tests/ProblemDetails.Tests
```

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
