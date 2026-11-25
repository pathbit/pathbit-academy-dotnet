# Bounded queues with Channels in ASP.NET Core

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

An API queues simulated work in memory and signals when capacity runs out. The lab runs in a local process without a broker, database, or credentials.

[Read the article](article/ARTICLE.en.md) · [.NET catalogue](../README.en.md)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- [curl to run the HTTP commands in a terminal](https://curl.se/download.html).

## Quick start

Start the API on loopback from this lab directory.

```bash
dotnet run --project src/BoundedJobs.Api --no-launch-profile -- --urls http://127.0.0.1:19805
```

## Saturate the queue and observe recovery

In a second terminal, use a Bash-compatible shell to send six concurrent requests. With one running job and two waiting, pressure produces `503` with `Retry-After: 1` as well as `202` with accepted IDs. Compare them with the completed IDs in the server log. The number of `503` responses may vary with local scheduling.

```bash
for ((request=0; request<6; request++)); do
  (
    response=$(curl -sS -w ' %{http_code}' http://127.0.0.1:19805/jobs \
      -H 'Content-Type: application/json' -d '{"units":5}')
    printf '%s\n' "$response"
  ) &
done
wait
```

In the server log, wait for a `Completed job` line for each accepted `id`.
Submit another job and confirm the `202` response. The batch's `wait` command
waits only for the `curl` processes.

```bash
curl -si -w '\n' http://127.0.0.1:19805/jobs \
  -H 'Content-Type: application/json' -d '{"units":1}'
```

Press `Ctrl+C` to stop the API.

## Run the tests

The checks use a controlled processor to keep the queue full without relying on timed sleeps.

```bash
dotnet test ../tests/BoundedJobs.Tests
```

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
