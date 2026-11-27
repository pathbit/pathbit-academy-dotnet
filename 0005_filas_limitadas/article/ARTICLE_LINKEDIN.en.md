# Put a bound on background work with Channels in ASP.NET Core

A service accepts preview-generation requests faster than it can process them. Moving work out of the request helps latency, but the service still needs to decide how much work to accept under pressure.

## Design the behavior

The sample `Channel<T>` has two waiting slots and one consumer. Once the consumer takes a job, it can process it while two more wait. With `BoundedChannelFullMode.Wait`, the endpoint uses `TryWrite` to get an immediate result. `WriteAsync` would wait for capacity.

## Run the lab

Run the concurrent batch and match accepted IDs with completion logs. Submit another task once a slot frees. Tests use a controlled worker to reproduce saturation and recovery. In-memory jobs are local to a process and are lost on restart. Use durable storage for work that needs to outlive the process.

[Full guide](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0005_filas_limitadas/article/ARTICLE.en.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
