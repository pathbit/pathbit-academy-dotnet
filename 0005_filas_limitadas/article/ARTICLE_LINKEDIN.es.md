# Limita las tareas en segundo plano con Channels en ASP.NET Core

Un servicio recibe solicitudes de previsualización más rápido de lo que puede procesarlas. Sacar el trabajo de la petición mejora la latencia, pero el servicio aún debe decidir cuánto acepta bajo presión.

## Diseña el comportamiento

El `Channel<T>` del ejemplo guarda dos tareas en espera y tiene un consumidor. Cuando el consumidor toma una tarea, puede procesarla mientras esperan otras dos. Con `BoundedChannelFullMode.Wait`, `TryWrite` da una respuesta inmediata. `WriteAsync` podría esperar un espacio.

## Ejecuta el laboratorio

Ejecuta el lote concurrente y cruza los IDs aceptados con los registros del consumidor. Envía otra tarea cuando se libere un espacio. Las pruebas usan un consumidor controlado. Las tareas en memoria pertenecen al proceso y se pierden al reiniciar. Usa almacenamiento duradero si deben sobrevivir.

[Guía completa](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0005_filas_limitadas/article/ARTICLE.es.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
