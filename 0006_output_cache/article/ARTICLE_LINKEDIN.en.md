# Reuse catalog responses with ASP.NET Core Output Caching

A store exposes brief and full catalog views. Both should show the new price after an update, while unrelated pages keep their cached responses. The lab models reuse, cache keys, and invalidation through observable fields.

## Design the behavior

The catalog policy varies by `view` and by catalog revision. `brief`, `full`, and `/catalog/summary` share a `catalog` tag. `/about` uses its own tag. A `generation` changes when a handler builds a response. A `revision` identifies the product state used by that handler.

## Run the lab

Follow the recorded sequence. Read `brief` twice, check `full` and the summary, and change the price to `12900` cents. Confirm the new revision on the catalog routes and stable generation on `/about` within the TTL. With multiple replicas, use a coherent revision source and choose a store that matches your invalidation policy.

[Full guide](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0006_output_cache/article/ARTICLE.en.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
