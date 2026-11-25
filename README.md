# pathbit-academy-dotnet

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

Conteúdos técnicos sobre .NET publicados pela Pathbit

## Pré-requisitos

- Obrigatório para compilar e testar a solução. [SDK do .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- Para os comandos HTTP nos laboratórios. [curl](https://curl.se/download.html).
- Para as bancadas em contêiner dos artigos 0001 e 0003. [Docker com Compose](https://docs.docker.com/get-started/get-docker/).

## Começo rápido

Na raiz do clone, execute os testes da solução. Para subir uma API e fazer as requisições manualmente, abra o README do laboratório abaixo.

```bash
dotnet test
```

## 📚 Artigos e laboratórios

### [0001 - Centralizando e organizando logs no .NET com Serilog e Seq](./0001_serilog_seq_logging)

**Ano:** 2025
**Autor:** Eliel Sousa
**Categoria:** .NET / Logging e Observabilidade

Aprenda a implementar logging estruturado em aplicações .NET usando Serilog e Seq. O artigo cobre desde a configuração básica até cenários práticos de troubleshooting, com exemplo completo de uma API que consome serviços externos e registra logs estruturados.

**Tecnologias:** .NET 10 LTS, Serilog, Seq, Docker

---

### [0002 - Versionamento de API no ASP.NET Core](./0002_versionamento_api)

**Categoria:** .NET / APIs

Exemplo executável com duas versões da mesma API (v1 deprecada e v2), rotas por versão na URL, headers `api-supported-versions` / `api-deprecated-versions` e um documento Swagger por versão.

**Tecnologias:** .NET 10 LTS, Microsoft.AspNetCore.Mvc.Versioning, Swashbuckle

---

### [0003 - Liveness e readiness no ASP.NET Core](./0003_liveness_readiness)

**Categoria:** .NET / Observabilidade

Laboratório com duas verificações de saúde e uma dependência HTTP que pode ser interrompida e restaurada no Docker Compose.

**Tecnologias:** .NET 10 LTS, ASP.NET Core Health Checks, Docker Compose

---

### [0004 - Contratos de erro HTTP com Problem Details](./0004_problem_details)

Uma API de prévia de reservas separa validação, produto inexistente, conflito de estoque e falha interna. Compare as respostas HTTP e verifique os campos usados pelo cliente.

**Tecnologias:** .NET 10 LTS, Minimal APIs, RFC 9457

---

### [0005 - Filas limitadas com Channels no ASP.NET Core](./0005_filas_limitadas)

Uma API aceita tarefas em memória, responde com `503` ao saturar e volta a aceitar trabalho quando o consumidor libera espaço. Os testes reproduzem a sobrecarga sem esperas fixas.

**Tecnologias:** .NET 10 LTS, Channels, BackgroundService

---

### [0006 - Output caching com invalidação por tag no ASP.NET Core](./0006_output_cache)

Um catálogo armazena respostas por variante e revisão. A alteração de preço atualiza as respostas relacionadas e preserva uma entrada independente do cache.

**Tecnologias:** .NET 10 LTS, Minimal APIs, ASP.NET Core Output Cache

---

## 🎯 Sobre o Pathbit Academy

O Pathbit Academy é uma iniciativa da Pathbit para compartilhar conhecimento técnico de qualidade, com foco em aplicações práticas e exemplos reais. Todos os artigos incluem código fonte completo e instruções detalhadas para execução.

## 📖 Como Usar

Cada artigo possui sua própria pasta com a seguinte estrutura:

```
000X_nome_do_artigo/
├── README.md              # Instruções de execução
├── article/
│   └── ARTICLE.md        # Conteúdo do artigo
├── assets/               # Imagens e diagramas, quando aplicável
└── src/                  # Código fonte completo
```

## 📧 Contato

- **Site:** [Pathbit](https://www.pathbit.com.br/)
- **LinkedIn:** [Pathbit](https://linkedin.com/company/pathbit)

---

## Contributing and Branch Protection

Envie suas alterações em um pull request do seu fork para `master` para revisão. Execute as verificações aplicáveis ao conteúdo ou ao laboratório antes de pedir revisão.
Para informar um erro ou sugerir um artigo, [abra uma issue](https://github.com/pathbit/pathbit-academy-dotnet/issues).

---

## 📄 License

Distribuído sob a **Licença MIT**. O texto completo está em [LICENSE](./LICENSE).

---

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
