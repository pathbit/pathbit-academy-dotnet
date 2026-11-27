# Como aceitar tarefas em segundo plano sem deixar a fila crescer sem limite

[English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Imagine uma API que gera prévias pesadas. Em condições normais as solicitações chegam devagar. Se seis pessoas clicarem juntas, fazer tudo na requisição prende as conexões. Colocar tudo numa fila ilimitada só muda onde a espera se acumula. Um `Channel<T>` limitado permite controlar o volume que o processo aceita e informar ao cliente quando tentar mais tarde.

## Calcule a capacidade a partir da vida de uma tarefa

O [canal do laboratório](../src/BoundedJobs.Api/JobQueue.cs) guarda duas tarefas em espera. O único consumidor retira uma tarefa e a processa fora do canal. Com uma em execução, ainda cabem duas aguardando. Um terceiro item aguardando excederia a capacidade configurada.

| Estado | Em execução | Aguardando | Próximo `POST` válido |
| --- | ---: | ---: | --- |
| Consumidor ocupado, fila vazia | 1 | 0 | `202` |
| Dois itens já aguardam | 1 | 2 | `503` com `Retry-After: 1` |
| O consumidor retirou o próximo | 1 | 1 | `202` |

O canal usa `BoundedChannelFullMode.Wait`. O endpoint verifica `TryWrite`, que responde imediatamente se não houver espaço disponível. Uma implementação que use `WriteAsync` pode esperar por espaço. Em uma API síncrona isso muda a latência e exige que o cancelamento da requisição faça parte do desenho.

## Diferencie aceitação de conclusão

`POST /jobs` retorna `202` com o `id` da tarefa aceita. O [BackgroundService](../src/BoundedJobs.Api/JobWorker.cs) registra `Completed job` com aquele identificador. Se `units` estiver fora de 1 a 5, o endpoint responde `400`. Se a fila estiver indisponível por lotação, o `503` pede espera, e o valor de `Retry-After` é uma sugestão para a próxima tentativa.

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

## Reproduza a sobrecarga e a recuperação

Siga o [guia](../README.md). Envie o lote, copie os IDs de `202` e compare com o log. Após a conclusão de pelo menos um item, mande nova tarefa. Os [testes](../../tests/BoundedJobs.Tests/JobContracts.cs) controlam a primeira tarefa com um sinal para checar a lotação e a ordem sem depender do escalonamento do sistema.

## Decida onde a tarefa precisa viver

O canal desta aula vive no processo. Uma reinicialização perde itens pendentes, e cada réplica tem a própria fila. Use um broker ou armazenamento durável quando o trabalho precisar sobreviver a reinícios ou ser compartilhado. O desenho do consumidor precisa definir confirmação, retentativa e recuperação de falhas.

[Documentação de Channels no .NET](https://learn.microsoft.com/pt-br/dotnet/core/extensions/channels)

---

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
