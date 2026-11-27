# Uma fila com dois lugares pode ter três tarefas aceitas

No laboratório, duas tarefas esperam no Channel e uma fica em processamento. Quando chega outra solicitação, a API responde `503` com `Retry-After` e deixa o consumidor avançar.

## Calcule a capacidade a partir da vida de uma tarefa

O [canal do laboratório](https://github.com/pathbit/pathbit-academy-dotnet/blob/master/0005_filas_limitadas/src/BoundedJobs.Api/JobQueue.cs) guarda duas tarefas em espera. O único consumidor retira uma tarefa e a processa fora do canal. Com uma em execução, ainda cabem duas aguardando. Um terceiro item aguardando excederia a capacidade configurada.

| Estado | Em execução | Aguardando | Próximo `POST` válido |
| --- | ---: | ---: | --- |
| Consumidor ocupado, fila vazia | 1 | 0 | `202` |
| Dois itens já aguardam | 1 | 2 | `503` com `Retry-After: 1` |
| O consumidor retirou o próximo | 1 | 1 | `202` |

O canal usa `BoundedChannelFullMode.Wait`. O endpoint verifica `TryWrite`, que responde imediatamente se não houver espaço disponível. Uma implementação que use `WriteAsync` pode esperar por espaço. Em uma API síncrona isso muda a latência e exige que o cancelamento da requisição faça parte do desenho.

## Diferencie aceitação de conclusão

`POST /jobs` retorna `202` com o `id` da tarefa aceita. O [BackgroundService](https://github.com/pathbit/pathbit-academy-dotnet/blob/master/0005_filas_limitadas/src/BoundedJobs.Api/JobWorker.cs) registra `Completed job` com aquele identificador. Se `units` estiver fora de 1 a 5, o endpoint responde `400`. Se a fila estiver indisponível por lotação, o `503` pede espera, e o valor de `Retry-After` é uma sugestão para a próxima tentativa.

Em um cliente automatizado, limite as tentativas e use espera crescente. Repetir uma operação cuja resposta foi perdida pode enfileirar uma segunda tarefa. Se esse risco importa, modele uma chave de idempotência antes de automatizar retentativas.

```csharp
var job = new Job(Guid.NewGuid(), request.Units);
if (!queue.TryEnqueue(job))
{
    context.Response.Headers.RetryAfter = "1";
    return Results.Problem(
        statusCode: StatusCodes.Status503ServiceUnavailable,
        type: "urn:pathbit:problem:job-queue-full",
        title: "The job queue is full.");
}

return Results.Accepted(value: new { id = job.Id });
```

## Reproduza o cenário no laboratório

Execute o lote concorrente e acompanhe cada `id` aceito até `Completed job`. Uma resposta `202` confirma que a tarefa entrou. O evento mostra quando ela terminou.

[Artigo completo e laboratório no GitHub](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0005_filas_limitadas)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
