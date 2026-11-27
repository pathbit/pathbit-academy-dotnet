# ASP.NET Core output caching with tag-based eviction

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

A catalog API reuses HTTP responses and refreshes only the representations affected by a price change. The lab uses ASP.NET Core's in-memory cache and requires no credentials.

[Read the article](article/ARTICLE.en.md) · [.NET catalogue](../README.en.md)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- [curl to run the HTTP commands in a terminal](https://curl.se/download.html).

## Quick start

Start the API on loopback from this lab directory.

```bash
dotnet run --project src/CatalogCache.Api --no-launch-profile -- --urls http://127.0.0.1:19806
```

## Observe cache hits and eviction

Send the requests in a second terminal. The first two responses share the same `generation`. The `full` variant and summary route have separate generations. After the `POST`, the catalog routes return `priceInCents: 12900`, a new `revision`, and new `generation` values. The `/about` response keeps its previous generation.

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

Press `Ctrl+C` to stop the API.

## Run the tests

The tests cover query variants, selective eviction, and revision isolation without waiting for the TTL to expire.

```bash
dotnet test ../tests/CatalogCache.Tests
```

[LinkedIn article](article/ARTICLE_LINKEDIN.en.md) · [LinkedIn post](article/POST_LINKEDIN.en.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
