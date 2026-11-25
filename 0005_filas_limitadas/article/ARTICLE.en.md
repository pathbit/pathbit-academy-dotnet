# Bounded queues with Channels in ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

An in-memory queue separates task submission from processing. When arrivals exceed processing capacity, a queue limit keeps the number of accepted jobs predictable. The explicit limit also gives the client a response when it should wait before trying again.

## Distinguish running work from waiting work

The API has one consumer processing one job at a time. The queue holds at most two jobs that have yet to start. When the consumer is busy, the instance can hold up to three accepted jobs, one running and two waiting. A job frees its queue slot when the consumer reads it.

The [channel](../src/BoundedJobs.Api/JobQueue.cs) uses `BoundedChannelFullMode.Wait`, `SingleReader = true`, and `SingleWriter = false`. The `Wait` mode avoids automatic dropping. The endpoint uses `TryWrite` because the HTTP request can get an immediate answer. When the channel is full, `TryWrite` returns `false` instead of waiting for space or replacing an accepted job.

## Respond with the observed capacity

| Outcome for `POST /jobs` | Response | Interpretation |
| --- | --- | --- |
| `units` outside 1–5 | `400` | Correct the input |
| Job placed on the queue | `202` and `id` | The instance accepted the job for processing |
| Queue has no space | `503` and `Retry-After: 1` | Try again after the suggested delay |

The `202` response confirms entry into the queue. Completion appears in the [processor](../src/BoundedJobs.Api/JobWorker.cs) log with the same identifier. The one-second hint for a `503` may be insufficient under sustained load. An automated client can combine bounded retries with growing delays. To retry after losing a response, the contract would need an idempotency key to avoid duplicate jobs.

## Process work outside the request lifecycle

The `BackgroundService` drains the channel through `ReadAllAsync` in channel arrival order. A job failure is logged and the consumer continues to the next job. The host shutdown token interrupts the simulated processor on shutdown.

Every instance has its own channel and its jobs disappear when the process exits. For a workflow that needs to survive restarts or share work across replicas, use durable storage and define acknowledgement, retry, and idempotency semantics.

## Observe overload and recovery

Run the [lab](../README.en.md), submit the batch, and let the consumer free some space. Compare the accepted IDs with those in the server log. The [tests](../../tests/BoundedJobs.Tests/JobContracts.cs) hold the first job with an explicit signal, fill the other two slots, and confirm the `503`. Then they release the processor, validate ordering, and verify that a failing job does not stop the next one.

[Channels documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
