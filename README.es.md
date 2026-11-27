# pathbit-academy-dotnet

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

Artículos prácticos de .NET de Pathbit, con código ejecutable y pruebas automatizadas.

## Requisitos previos

- Obligatorio para compilar y probar la solución. [SDK de .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- Para los comandos HTTP de los laboratorios. [curl](https://curl.se/download.html).
- Para los laboratorios de contenedores de los artículos 0001 y 0003. [Docker con Compose](https://docs.docker.com/get-started/get-docker/).

## Inicio rápido

Ejecuta las pruebas de la solución desde la raíz del clon. Para iniciar una API y enviar solicitudes manualmente, abre la guía de su laboratorio.

```bash
dotnet test
```

## 📚 Artículos y laboratorios

| Artículo | Qué vas a probar |
| --- | --- |
| [0001 - Registros estructurados con Serilog y Seq (PT-BR)](0001_serilog_seq_logging/README.md) | Centraliza los registros estructurados de una API que consulta un servicio externo. |
| [0002 - Versionado de API en ASP.NET Core (PT-BR)](0002_versionamento_api/README.md) | Ejecuta dos versiones de una API y revisa las cabeceras y los documentos de Swagger. |
| [0003 - Liveness y readiness en ASP.NET Core](0003_liveness_readiness/README.es.md) | Detén y recupera una dependencia HTTP local para observar las comprobaciones de salud. |
| [0004 - Contratos de error HTTP con Problem Details](0004_problem_details/README.es.md) | Inspecciona errores de validación y conflictos de stock con el formato de la RFC 9457. |
| [0005 - Colas acotadas con Channels en ASP.NET Core](0005_filas_limitadas/README.es.md) | Llena una cola acotada y observa cómo la API se recupera cuando el consumidor libera espacio. |
| [0006 - Caché de respuestas con invalidación por etiqueta en ASP.NET Core](0006_output_cache/README.es.md) | Compara variantes, cambia un precio y observa la invalidación selectiva. |

Cada laboratorio tiene un README con instrucciones, un artículo en `article/` y el código de la aplicación en `src/`. La solución incluye pruebas automatizadas en `tests/`. Los laboratorios también pueden incluir diagramas y recursos de apoyo.

## Contacto

[Sitio de Pathbit](https://www.pathbit.com.br/) · [Pathbit en LinkedIn](https://linkedin.com/company/pathbit)

---

## Contributing and Branch Protection

Abre un pull request desde tu fork hacia `master` para revisión. Ejecuta las comprobaciones que correspondan al artículo o laboratorio antes de solicitar la revisión. Para informar de un problema o sugerir un artículo, [abre una issue](https://github.com/pathbit/pathbit-academy-dotnet/issues).

---

## 📄 License

Distribuido bajo la **licencia MIT**. El texto completo está en [LICENSE](LICENSE).

---

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
