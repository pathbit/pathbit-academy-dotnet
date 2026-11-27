# Separa liveness y readiness en ASP.NET Core

Durante un despliegue, el proceso de la API arranca mientras el servicio de inventario sigue caído. Una sola ruta de salud dificulta que el orquestador decida si debe retirar tráfico o reiniciar el proceso.

## Diseña el comportamiento

El ejercicio configura `/health/live` con un predicado vacío. La respuesta confirma que el proceso atiende la solicitud. `/health/ready` ejecuta las comprobaciones con la etiqueta `ready`. El cliente HTTP espera hasta dos segundos y considera saludable una respuesta `2xx`.

## Ejecuta el laboratorio

Usa comprobaciones cortas para las dependencias necesarias al atender tráfico. Una sonda de readiness fallida puede retirar un endpoint del Service mientras el proceso tiene oportunidad de recuperarse.

[Guía completa](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0003_liveness_readiness/article/ARTICLE.es.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
