# Output caching com invalidação por tag no ASP.NET Core

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

Uma API de catálogo reutiliza respostas HTTP e atualiza somente as representações afetadas quando o preço muda. O laboratório usa o cache em memória do ASP.NET Core e não precisa de credenciais.

[Leia o artigo](article/ARTICLE.md) · [Catálogo .NET](../README.md)

## Pré-requisitos

- [SDK do .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- [curl para executar os comandos HTTP em um terminal](https://curl.se/download.html).

## Começo rápido

Na pasta deste laboratório, inicie a API em loopback.

```bash
dotnet run --project src/CatalogCache.Api --no-launch-profile -- --urls http://127.0.0.1:19806
```

## Observe o reaproveitamento e a invalidação

Envie as requisições em outro terminal. As duas primeiras respostas usam o mesmo `generation`; a variante `full` e a rota de resumo têm suas próprias gerações. Depois do `POST`, as rotas do catálogo devolvem `priceInCents: 12900`, uma nova `revision` e novos valores de `generation`. A resposta de `/about` mantém a geração anterior.

```bash
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=brief'
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=brief'
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=full'
curl -sS -w '\n' http://127.0.0.1:19806/catalog/summary
curl -sS -w '\n' http://127.0.0.1:19806/about
curl -sS -w '\n' http://127.0.0.1:19806/catalog/price \
  -H 'Content-Type: application/json' -d '{"priceInCents":12900}'
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=brief'
curl -sS -w '\n' 'http://127.0.0.1:19806/catalog?view=full'
curl -sS -w '\n' http://127.0.0.1:19806/catalog/summary
curl -sS -w '\n' http://127.0.0.1:19806/about
```

Encerre a API com `Ctrl+C`.

## Execute os testes

Os testes verificam as variantes de consulta, a invalidação seletiva e a separação das revisões sem esperar o TTL expirar.

```bash
dotnet test ../tests/CatalogCache.Tests
```

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
