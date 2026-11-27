# Separate liveness and readiness in ASP.NET Core

During deployment, the API process starts while its stock service is still down. A single health URL makes it hard for an orchestrator to choose between retrying traffic and restarting the process.

## Design the behavior

The lab maps `/health/live` with an empty check predicate. A successful response proves that the process can handle the request. `/health/ready` runs registrations tagged `ready`. Its HTTP dependency check uses a two-second client timeout and treats a `2xx` response as healthy.

## Run the lab

Keep readiness checks short and limited to dependencies needed to serve traffic. A failed readiness probe can remove an endpoint from a Service while the running process has a chance to recover.

[Full guide](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0003_liveness_readiness/article/ARTICLE.en.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
