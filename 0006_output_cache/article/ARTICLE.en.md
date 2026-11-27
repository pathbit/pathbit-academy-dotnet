# Reuse catalog responses with ASP.NET Core Output Caching

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

A store exposes brief and full catalog views. Both should show the new price after an update, while unrelated pages keep their cached responses. The lab models reuse, cache keys, and invalidation through observable fields.

## Design the behavior

The catalog policy varies by `view` and by catalog revision. `brief`, `full`, and `/catalog/summary` share a `catalog` tag. `/about` uses its own tag. A `generation` changes when a handler builds a response. A `revision` identifies the product state used by that handler.

## Read the implementation

Capture the same immutable product for the cache key and the route body. The write endpoint evicts the tag `catalog`. A read that started before the write still uses the old key even if it finishes late. Subsequent reads use the new revision, and the TTL bounds the old entries.

[Source code](../src/CatalogCache.Api/Program.cs)

## Run the lab

Follow the recorded sequence. Read `brief` twice, check `full` and the summary, and change the price to `12900` cents. Confirm the new revision on the catalog routes and stable generation on `/about` within the TTL. With multiple replicas, use a coherent revision source and choose a store that matches your invalidation policy.

[Full guide](../README.en.md) · [Official documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
