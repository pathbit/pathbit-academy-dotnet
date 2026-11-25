# ASP.NET Core output caching with tag-based eviction

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Repeated requests to a catalog can reuse the same HTTP response without invoking the endpoint. A price change should refresh the catalog views that use the price. In this lab, a tagged policy defines the group and a revision in the entry key prevents a new read from retrieving data from an earlier revision.

## Identify what changes the representation

The [`catalog` policy](../src/CatalogCache.Api/Program.cs) uses `SetVaryByQuery("view")` because the `view` query changes the `GET /catalog` body. The `brief` and `full` variants have separate entries. The `/catalog/summary` route has its own path and uses the same policy. Middleware stores eligible responses, including the public `200` responses in this example. The `400` for an invalid variant is not stored.

The `generation` counter increments whenever the endpoint creates a response. Two reads of the same key normally return the same generation until expiration or eviction. Responses with different keys can have different generations while using the same catalog revision.

| Request | Dimensions to observe | Tag |
| --- | --- | --- |
| `GET /catalog?view=brief` | Path, `brief`, current revision | `catalog` |
| `GET /catalog?view=full` | Path, `full`, current revision | `catalog` |
| `GET /catalog/summary` | Path and current revision | `catalog` |
| `GET /about` | Independent path | `about` |

## Prevent reuse of stale revision data

The update publishes a new [immutable product](../src/CatalogCache.Api/Catalog.cs) with an incremented revision. Before consulting the cache, each request captures a reference to the revision visible at that moment. The middleware uses the revision in `VaryByValue` and the endpoint uses that same product to build the body. Reusing the reference prevents writing an old body with a new key.

The write route calls `EvictByTagAsync("catalog", ...)` to discard related representations. A read started before the change can still respond with the revision it captured. If it populates the cache after eviction, it writes under the old key and leaves later reads of the current revision unaffected. A two-minute TTL bounds the lifetime of those old entries.

## Verify selective eviction

In the [quick start](../README.en.md), make two identical reads, request the other variants, and change the price. Inspect the new `priceInCents` and `revision` in the catalog responses. Compare the `generation` of `/about` before and after the update. The price change leaves that content intact.

The [tests](../../tests/CatalogCache.Tests/CacheContracts.cs) exercise eviction by tag with a fixed revision and also change the revision without evicting by tag. These scenarios check the two guarantees independently.

## Choose storage to match the deployment

The lab catalog and cache live in the local process. With multiple instances, the revision needs a source that is consistent across replicas. Shared cache storage and the expiration policy should follow that contract. Use output caching for responses that can safely share one key. Personalized responses need a policy that respects user data and scope.

[ASP.NET Core output caching documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
