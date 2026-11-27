# Limita las tareas en segundo plano con Channels en ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Un servicio recibe solicitudes de previsualización más rápido de lo que puede procesarlas. Sacar el trabajo de la petición mejora la latencia, pero el servicio aún debe decidir cuánto acepta bajo presión.

## Diseña el comportamiento

El `Channel<T>` del ejemplo guarda dos tareas en espera y tiene un consumidor. Cuando el consumidor toma una tarea, puede procesarla mientras esperan otras dos. Con `BoundedChannelFullMode.Wait`, `TryWrite` da una respuesta inmediata. `WriteAsync` podría esperar un espacio.

## Lee la implementación

`POST /jobs` devuelve `202` y un ID cuando acepta una tarea. El evento de finalización lleva ese mismo ID. Un valor `units` inválido devuelve `400`. La cola llena devuelve `503` con `Retry-After: 1`. Limita los reintentos y diseña idempotencia antes de repetir solicitudes con resultado ambiguo.

[Código fuente](../src/BoundedJobs.Api/JobQueue.cs)

## Ejecuta el laboratorio

Ejecuta el lote concurrente y cruza los IDs aceptados con los registros del consumidor. Envía otra tarea cuando se libere un espacio. Las pruebas usan un consumidor controlado. Las tareas en memoria pertenecen al proceso y se pierden al reiniciar. Usa almacenamiento duradero si deben sobrevivir.

[Guía completa](../README.es.md) · [Documentación oficial](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
