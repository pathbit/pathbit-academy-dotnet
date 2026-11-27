# Separate liveness and readiness in ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

During deployment, the API process starts while its stock service is still down. A single health URL makes it hard for an orchestrator to choose between retrying traffic and restarting the process.

## Design the behavior

The lab maps `/health/live` with an empty check predicate. A successful response proves that the process can handle the request. `/health/ready` runs registrations tagged `ready`. Its HTTP dependency check uses a two-second client timeout and treats a `2xx` response as healthy.

## Read the implementation

Start Compose, then stop `dependency`. The API keeps returning `200` for liveness and returns `503` for readiness. Restore the dependency and wait until readiness returns `200`. In Kubernetes, inspect EndpointSlices and the restart count while the separate probes run.

[Source code](../src/HealthChecks.Api/Program.cs)

## Run the lab

Keep readiness checks short and limited to dependencies needed to serve traffic. A failed readiness probe can remove an endpoint from a Service while the running process has a chance to recover.

[Full guide](../README.en.md) · [Official documentation](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
