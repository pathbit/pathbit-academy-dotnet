# Como reutilizar respostas de catálogo com Output Caching no ASP.NET Core

[English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Uma página de catálogo pede a versão compacta de um produto muitas vezes. Outra página quer os detalhes. Ambas exibem o preço e precisam mudar depois que o lojista o salva. O cache de saída do ASP.NET Core pode reutilizar uma resposta HTTP completa. O desafio aqui é escolher quem compartilha a resposta e quais entradas devem sair quando o preço muda.

## Escreva as dimensões da chave

| Rota | O que varia | Tag |
| --- | --- | --- |
| `/catalog?view=brief` | Caminho, `view`, revisão do catálogo | `catalog` |
| `/catalog?view=full` | Caminho, `view`, revisão do catálogo | `catalog` |
| `/catalog/summary` | Caminho e revisão do catálogo | `catalog` |
| `/about` | Caminho próprio | `about` |

No [código da política](../src/CatalogCache.Api/Program.cs), `SetVaryByQuery("view")` separa os corpos curto e completo. `VaryByValue` leva em conta a revisão. O middleware guarda as respostas públicas elegíveis. No exemplo, um `400` de `view` inválido fica de fora.

`generation` aumenta apenas quando o handler monta a resposta. Se duas chamadas usam a mesma chave e a resposta ainda está no cache, você vê a mesma geração. `revision` descreve o estado do produto e pode aparecer em várias representações.

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy(catalogTag, policy => policy
        .Expire(TimeSpan.FromMinutes(2))
        .Tag(catalogTag)
        .SetVaryByQuery("view")
        .VaryByValue(context => new KeyValuePair<string, string>(
            "revision", context.Features.Get<Product>()!.Revision.ToString(CultureInfo.InvariantCulture))));
});
```

## Invalide a família de respostas quando o preço muda

O [catálogo](../src/CatalogCache.Api/Catalog.cs) publica um produto imutável com uma revisão nova. A leitura captura a referência antes de consultar o cache. Usa a mesma revisão para escolher a chave e o mesmo produto para montar o corpo. A rota de escrita chama `EvictByTagAsync("catalog", ...)`, que remove entradas associadas ao preço. A tag `about` fica fora do grupo.

Uma leitura que já capturou o produto anterior pode terminar depois da gravação. Se ela repovoar o cache, usa a chave da revisão antiga. A leitura seguinte usa a chave nova. O TTL de dois minutos limita por quanto tempo a entrada velha permanece. Esse detalhe importa em qualquer implementação que permita leituras durante a atualização.

## Observe o que reutiliza e o que muda

Faça as chamadas do [guia](../README.md) na ordem. Compare duas leituras de `brief`, depois `full` e `/catalog/summary`. Após escrever o preço de `12900` centavos, as três rotas do catálogo recebem a nova revisão e novas gerações. `/about` mantém a geração dentro da janela de expiração. Os [testes](../../tests/CatalogCache.Tests/CacheContracts.cs) isolam invalidação por tag e alteração de revisão.

## Leve em conta réplicas e dados personalizados

O cache e as revisões desta aula são locais ao processo. Para múltiplas instâncias, defina uma origem coerente para a revisão e um armazenamento compatível com a política de invalidação. Para respostas personalizadas, revise todas as dimensões que separam os consumidores antes de compartilhar uma entrada.

[Output Caching no ASP.NET Core](https://learn.microsoft.com/pt-br/aspnet/core/performance/caching/output?view=aspnetcore-10.0)

---

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
