# Separa liveness y readiness en ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Durante un despliegue, el proceso de la API arranca mientras el servicio de inventario sigue caído. Una sola ruta de salud dificulta que el orquestador decida si debe retirar tráfico o reiniciar el proceso.

## Diseña el comportamiento

El ejercicio configura `/health/live` con un predicado vacío. La respuesta confirma que el proceso atiende la solicitud. `/health/ready` ejecuta las comprobaciones con la etiqueta `ready`. El cliente HTTP espera hasta dos segundos y considera saludable una respuesta `2xx`.

## Lee la implementación

Inicia Compose y detén `dependency`. La API mantiene `200` en liveness y devuelve `503` en readiness. Restaura la dependencia y espera un `200` de readiness. En Kubernetes, inspecciona EndpointSlices y el contador de reinicios mientras actúan las sondas separadas.

[Código fuente](../src/HealthChecks.Api/Program.cs)

## Ejecuta el laboratorio

Usa comprobaciones cortas para las dependencias necesarias al atender tráfico. Una sonda de readiness fallida puede retirar un endpoint del Service mientras el proceso tiene oportunidad de recuperarse.

[Guía completa](../README.es.md) · [Documentación oficial](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
