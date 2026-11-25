# Output caching com invalidação por tag no ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Requisições repetidas ao catálogo podem servir a mesma resposta HTTP sem chamar o código da rota. Uma alteração de preço deve atualizar as representações do catálogo que usam esse preço. No laboratório, uma política com tag define esse grupo e uma revisão no identificador da entrada evita que uma leitura nova recupere dados de uma revisão anterior.

## Identifique o que muda a representação

A [política `catalog`](../src/CatalogCache.Api/Program.cs) usa `SetVaryByQuery("view")` porque a consulta `view` altera o corpo de `GET /catalog`. As variantes `brief` e `full` têm entradas próprias. A rota `/catalog/summary` tem outro caminho e usa a mesma política. O middleware só guarda respostas elegíveis, como as respostas públicas `200` deste exemplo. A resposta `400` para uma variante inválida não é armazenada.

O contador `generation` é incrementado quando o handler produz uma resposta. Duas leituras da mesma chave normalmente devolvem a mesma geração até a expiração ou invalidação. Respostas a chaves diferentes podem ter gerações diferentes mesmo que tenham a mesma revisão do catálogo.

| Requisição | Dimensão observada | Tag |
| --- | --- | --- |
| `GET /catalog?view=brief` | Caminho, `brief`, revisão atual | `catalog` |
| `GET /catalog?view=full` | Caminho, `full`, revisão atual | `catalog` |
| `GET /catalog/summary` | Caminho e revisão atual | `catalog` |
| `GET /about` | Caminho independente | `about` |

## Evite reaproveitar dados de uma revisão antiga

A alteração publica um novo [produto imutável](../src/CatalogCache.Api/Catalog.cs) com revisão incrementada. Antes de consultar o cache, cada requisição captura uma referência à revisão visível naquele momento. O middleware usa essa revisão em `VaryByValue` e a rota lê o mesmo produto para montar o corpo. Usar a mesma referência evita gravar o corpo antigo sob a chave nova.

A rota de escrita chama `EvictByTagAsync("catalog", ...)` para descartar as representações relacionadas. Uma leitura iniciada antes da mudança ainda pode responder com a revisão que capturou. Se ela preencher o cache depois da invalidação, grava usando a chave antiga e não afeta uma leitura posterior com a revisão atual. O TTL de dois minutos limita a permanência dessas entradas antigas.

## Verifique a invalidação seletiva

No [começo rápido](../README.md), faça duas leituras iguais, consulte as outras variantes e altere o preço. Confira o novo `priceInCents` e a nova `revision` nas respostas de catálogo. Compare o `generation` de `/about` antes e depois. A mudança de preço não afeta esse conteúdo.

Os [testes](../../tests/CatalogCache.Tests/CacheContracts.cs) exercitam diretamente a invalidação pela tag com revisão fixa e também mudam a revisão sem invalidar a tag. Essas situações verificam as duas garantias separadamente.

## Escolha o armazenamento conforme a implantação

O catálogo e o cache do laboratório vivem no processo local. Com múltiplas instâncias, a revisão precisa vir de uma fonte coerente entre réplicas. O armazenamento de cache compartilhado e a política de expiração devem acompanhar esse contrato. Use cache de saída para respostas compatíveis com a mesma chave. Respostas personalizadas precisam de uma política que respeite os dados e o escopo de cada usuário.

[Documentação de output caching no ASP.NET Core](https://learn.microsoft.com/pt-br/aspnet/core/performance/caching/output?view=aspnetcore-10.0)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
