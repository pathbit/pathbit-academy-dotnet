# Como atualizar o preço sem limpar o cache inteiro

O catálogo tem versões breve e completa. As duas variam pela mesma revisão e usam a tag `catalog`. Uma página independente usa a tag `about`.

## Escreva as dimensões da chave

| Rota | O que varia | Tag |
| --- | --- | --- |
| `/catalog?view=brief` | Caminho, `view`, revisão do catálogo | `catalog` |
| `/catalog?view=full` | Caminho, `view`, revisão do catálogo | `catalog` |
| `/catalog/summary` | Caminho e revisão do catálogo | `catalog` |
| `/about` | Caminho próprio | `about` |

No [código da política](https://github.com/pathbit/pathbit-academy-dotnet/blob/master/0006_output_cache/src/CatalogCache.Api/Program.cs), `SetVaryByQuery("view")` separa os corpos curto e completo. `VaryByValue` leva em conta a revisão. O middleware guarda as respostas públicas elegíveis. No exemplo, um `400` de `view` inválido fica de fora.

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

O [catálogo](https://github.com/pathbit/pathbit-academy-dotnet/blob/master/0006_output_cache/src/CatalogCache.Api/Catalog.cs) publica um produto imutável com uma revisão nova. A leitura captura a referência antes de consultar o cache. Usa a mesma revisão para escolher a chave e o mesmo produto para montar o corpo. A rota de escrita chama `EvictByTagAsync("catalog", ...)`, que remove entradas associadas ao preço. A tag `about` fica fora do grupo.

Uma leitura que já capturou o produto anterior pode terminar depois da gravação. Se ela repovoar o cache, usa a chave da revisão antiga. A leitura seguinte usa a chave nova. O TTL de dois minutos limita por quanto tempo a entrada velha permanece. Esse detalhe importa em qualquer implementação que permita leituras durante a atualização.

## Reproduza o cenário no laboratório

Faça as leituras antes e depois de atualizar o preço. A revisão e as gerações do catálogo devem mudar. A geração de `/about` fica igual dentro do TTL. O experimento mostra o alcance da invalidação.

[Artigo completo e laboratório no GitHub](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0006_output_cache)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
