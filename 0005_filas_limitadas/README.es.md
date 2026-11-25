# Colas acotadas con Channels en ASP.NET Core

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

Una API encola tareas simuladas en memoria y avisa cuando se agota la capacidad. El laboratorio se ejecuta en un proceso local sin intermediario, base de datos ni credenciales.

[Lee el artículo](article/ARTICLE.es.md) · [Catálogo .NET](../README.es.md)

## Requisitos previos

- [SDK de .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- [curl para ejecutar los comandos HTTP en una terminal](https://curl.se/download.html).

## Inicio rápido

Inicia la API en loopback desde el directorio del laboratorio.

```bash
dotnet run --project src/BoundedJobs.Api --no-launch-profile -- --urls http://127.0.0.1:19805
```

## Satura la cola y observa la recuperación

Desde otra terminal, usa un shell compatible con Bash para enviar seis solicitudes simultáneas. Con una tarea en ejecución y dos esperando, la presión produce respuestas `503` con `Retry-After: 1` y respuestas `202` con los identificadores aceptados. Contrástalos con los identificadores completados del registro. El número de respuestas `503` puede variar según la planificación local.

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

En el registro del servidor, espera una línea `Completed job` por cada `id`
aceptado. Envía otra tarea y confirma la respuesta `202`. El comando `wait`
del lote solo espera a los procesos `curl`.

```bash
curl -si -w '\n' http://127.0.0.1:19805/jobs \
  -H 'Content-Type: application/json' -d '{"units":1}'
```

Pulsa `Ctrl+C` para detener la API.

## Ejecuta las pruebas

Las comprobaciones usan un procesador controlado para llenar la cola sin depender de pausas cronometradas.

```bash
dotnet test ../tests/BoundedJobs.Tests
```

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
