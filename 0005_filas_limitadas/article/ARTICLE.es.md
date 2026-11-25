# Colas acotadas con Channels en ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Una cola en memoria separa la recepción de una tarea de su procesamiento. Cuando llegan más tareas de las que se pueden procesar, el límite mantiene previsible el número de tareas aceptadas. El límite explícito también permite avisar al cliente de cuándo debe esperar antes de volver a intentarlo.

## Distingue las tareas activas de las que esperan

La API tiene un consumidor que procesa una tarea a la vez. La cola guarda hasta dos tareas que todavía no han empezado. Con el consumidor ocupado, la instancia admite hasta tres tareas, una en ejecución y dos esperando. La tarea libera su espacio en la cola cuando el consumidor la lee.

El [canal](../src/BoundedJobs.Api/JobQueue.cs) usa `BoundedChannelFullMode.Wait`, `SingleReader = true` y `SingleWriter = false`. El modo `Wait` evita el descarte automático. El endpoint usa `TryWrite` porque puede dar una respuesta inmediata a la solicitud HTTP. Cuando el canal está lleno, `TryWrite` devuelve `false` sin esperar un espacio ni sustituir tareas aceptadas.

## Responde según la capacidad observada

| Resultado de `POST /jobs` | Respuesta | Interpretación |
| --- | --- | --- |
| `units` fuera de 1–5 | `400` | Corrige la entrada |
| Tarea incorporada a la cola | `202` e `id` | La instancia aceptó la tarea para procesarla |
| Cola sin espacio | `503` y `Retry-After: 1` | Inténtalo tras la espera sugerida |

La respuesta `202` confirma la entrada en la cola. La finalización aparece en el registro del [procesador](../src/BoundedJobs.Api/JobWorker.cs) con el mismo identificador. El segundo sugerido para repetir una respuesta `503` puede resultar insuficiente con una carga sostenida. Un cliente automatizado puede limitar los reintentos y espaciar cada uno más que el anterior. Para volver a enviar una solicitud cuya respuesta se perdió, el contrato necesitaría una clave de idempotencia que impida tareas duplicadas.

## Procesa las tareas fuera del ciclo de vida de la solicitud

`BackgroundService` lee el canal con `ReadAllAsync` siguiendo el orden de llegada al canal. Si una tarea falla, se registra el error y el consumidor pasa a la siguiente. El token de apagado del host interrumpe el procesamiento simulado cuando se detiene el servidor.

Cada instancia tiene su propio canal y las tareas se pierden cuando termina el proceso. Si el flujo debe sobrevivir a reinicios o repartir trabajo entre réplicas, usa almacenamiento duradero y define reglas de confirmación, reintento e idempotencia.

## Observa la sobrecarga y la recuperación

Ejecuta el [laboratorio](../README.es.md), envía el lote y deja que el consumidor libere espacio. Compara los identificadores aceptados con los que aparecen en el registro. Las [pruebas](../../tests/BoundedJobs.Tests/JobContracts.cs) retienen la primera tarea con una señal explícita, llenan los otros dos espacios y confirman el `503`. Después liberan el procesador, comprueban el orden y verifican que el fallo de una tarea no detiene la siguiente.

[Documentación de Channels](https://learn.microsoft.com/es-es/dotnet/core/extensions/channels)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
