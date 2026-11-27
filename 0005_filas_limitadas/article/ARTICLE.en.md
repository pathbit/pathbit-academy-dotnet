# Put a bound on background work with Channels in ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

A service accepts preview-generation requests faster than it can process them. Moving work out of the request helps latency, but the service still needs to decide how much work to accept under pressure.

## Design the behavior

The sample `Channel<T>` has two waiting slots and one consumer. Once the consumer takes a job, it can process it while two more wait. With `BoundedChannelFullMode.Wait`, the endpoint uses `TryWrite` to get an immediate result. `WriteAsync` would wait for capacity.

## Read the implementation

`POST /jobs` returns `202` and an ID once a job enters the queue. The completion event contains that ID. Invalid `units` return `400`. A full queue returns `503` with `Retry-After: 1`. For real retrying clients, set a limit and design idempotency before retrying after ambiguous failures.

[Source code](../src/BoundedJobs.Api/JobQueue.cs)

## Run the lab

Run the concurrent batch and match accepted IDs with completion logs. Submit another task once a slot frees. Tests use a controlled worker to reproduce saturation and recovery. In-memory jobs are local to a process and are lost on restart. Use durable storage for work that needs to outlive the process.

[Full guide](../README.en.md) · [Official documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
