# Contratos de erro HTTP com Problem Details

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

Uma API de prévia de reservas devolve erros de validação, ausência de produto e falta de estoque no formato da RFC 9457. O laboratório usa dados em memória e não precisa de credenciais.

[Leia o artigo](article/ARTICLE.md) · [Catálogo .NET](../README.md)

## Pré-requisitos

- [SDK do .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- [curl para executar os comandos HTTP em um terminal](https://curl.se/download.html).

## Começo rápido

Na pasta deste laboratório, inicie a API em loopback.

```bash
dotnet run --project src/ProblemDetails.Api --no-launch-profile -- --urls http://127.0.0.1:19804
```

## Experimente o contrato

Envie as requisições em outro terminal. A primeira retorna `200` com a prévia. As quatro seguintes retornam `400`, `404`, `409` e `500`. Confira `Content-Type`, `type`, `status`, `traceId` e os campos específicos de cada erro.

```bash
curl -si -w '\n' http://127.0.0.1:19804/reservations/preview \
  -H 'Content-Type: application/json' -d '{"sku":"keyboard","quantity":2}'
curl -si -w '\n' http://127.0.0.1:19804/reservations/preview \
  -H 'Content-Type: application/json' -d '{"sku":"","quantity":0}'
curl -si -w '\n' http://127.0.0.1:19804/reservations/preview \
  -H 'Content-Type: application/json' -d '{"sku":"mouse","quantity":1}'
curl -si -w '\n' http://127.0.0.1:19804/reservations/preview \
  -H 'Content-Type: application/json' -d '{"sku":"keyboard","quantity":3}'
curl -si -w '\n' http://127.0.0.1:19804/operations/fail
```

Encerre a API com `Ctrl+C`.

## Execute os testes

Na pasta deste laboratório, rode o comando abaixo. Os testes sobem o servidor em memória, sem ocupar a porta do exemplo.

```bash
dotnet test ../tests/ProblemDetails.Tests
```

[Artigo para LinkedIn](article/ARTICLE_LINKEDIN.md) · [Post para LinkedIn](article/POST_LINKEDIN.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
