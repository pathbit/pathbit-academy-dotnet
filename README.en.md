# pathbit-academy-dotnet

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

Hands-on .NET articles from Pathbit, with runnable code and automated tests.

## Prerequisites

- Required to build and test the solution. [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- For HTTP commands in the lab guides. [curl](https://curl.se/download.html).
- For the container labs in articles 0001 and 0003. [Docker with Compose](https://docs.docker.com/get-started/get-docker/).

## Quick start

Run the solution tests from the clone root. To start an API and send requests manually, open its lab guide below.

```bash
dotnet test
```

## 📚 Articles and labs

| Article | What you will try |
| --- | --- |
| [0001 - Structured logging with Serilog and Seq (PT-BR)](0001_serilog_seq_logging/README.md) | Centralize structured logs from an API that calls an external service. |
| [0002 - API versioning in ASP.NET Core (PT-BR)](0002_versionamento_api/README.md) | Run two API versions and inspect version headers and Swagger documents. |
| [0003 - Liveness and readiness in ASP.NET Core (PT-BR)](0003_liveness_readiness/README.md) | Stop and restore a local HTTP dependency to observe the health checks. |
| [0004 - HTTP error contracts with Problem Details](0004_problem_details/README.en.md) | Inspect validation errors and stock conflicts in the RFC 9457 format. |
| [0005 - Bounded queues with Channels in ASP.NET Core](0005_filas_limitadas/README.en.md) | Fill a bounded queue and watch the API recover as the consumer frees space. |
| [0006 - ASP.NET Core output caching with tag-based eviction](0006_output_cache/README.en.md) | Compare query variants, change a price, and observe selective cache eviction. |

Each lab has a README with run instructions, an article in `article/`, and the application source in `src/`. The solution includes automated tests under `tests/`. Labs can also include supporting diagrams and assets.

## Contact

[Pathbit website](https://www.pathbit.com.br/) · [Pathbit on LinkedIn](https://linkedin.com/company/pathbit)

---

## Contributing and Branch Protection

Open a pull request from your fork to `master` for review. Run the checks that apply to the article or lab before requesting review. To report a bug or suggest an article, [open an issue](https://github.com/pathbit/pathbit-academy-dotnet/issues).

---

## 📄 License

Distributed under the **MIT License**. The full text is in [LICENSE](LICENSE).

---

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
