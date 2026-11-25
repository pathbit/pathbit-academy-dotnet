# Filas limitadas com Channels no ASP.NET Core

[Português (Brasil)](ARTICLE.md) · [English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Uma fila em memória separa o recebimento de uma tarefa do momento em que ela é processada. Quando a chegada supera a capacidade de processamento, limitar a fila mantém previsível o número de tarefas aceitas. Com um limite explícito, o cliente também recebe uma resposta quando deve esperar para tentar de novo.

## Distinga trabalho ativo de trabalho aguardando

A API tem um consumidor executando uma tarefa por vez. A fila guarda até duas tarefas que ainda não começaram. Com o consumidor ocupado, a instância mantém até três tarefas aceitas, uma em execução e duas esperando. A tarefa deixa de ocupar um espaço da fila quando o consumidor a lê.

O [canal](../src/BoundedJobs.Api/JobQueue.cs) usa `BoundedChannelFullMode.Wait`, `SingleReader = true` e `SingleWriter = false`. O modo `Wait` evita descarte automático. Usamos `TryWrite` no endpoint porque a requisição HTTP pode receber o resultado imediatamente. Com o canal cheio, `TryWrite` retorna `false` em vez de aguardar um espaço ou substituir uma tarefa já aceita.

## Responda pela capacidade observada

| Resultado em `POST /jobs` | Resposta | Como interpretar |
| --- | --- | --- |
| Campo `units` fora de 1–5 | `400` | Ajuste a entrada |
| Tarefa colocada na fila | `202` e `id` | A instância aceitou a tarefa para processamento |
| Fila sem espaço | `503` e `Retry-After: 1` | Tente novamente após a espera sugerida |

A resposta `202` confirma a entrada na fila. A conclusão aparece no log do [processador](../src/BoundedJobs.Api/JobWorker.cs) com o mesmo identificador. O segundo sugerido para repetir uma resposta `503` é uma dica e pode ser insuficiente sob carga contínua. Em um cliente automatizado, combine retentativas limitadas com espaçamento crescente. Para repetir depois de perder uma resposta, o contrato precisaria de uma chave de idempotência para evitar tarefas duplicadas.

## Processe sem depender do ciclo de vida da requisição

`BackgroundService` drena o canal com `ReadAllAsync`, seguindo a ordem de chegada ao canal. A falha de uma tarefa é registrada e o consumidor segue para a próxima. O token de encerramento do host interrompe o processamento simulado na parada.

Cada instância tem seu próprio canal e suas tarefas deixam de existir quando o processo encerra. Em um fluxo que precisa sobreviver a reinicializações ou compartilhar trabalho entre réplicas, use um armazenamento durável e defina um contrato de confirmação, repetição e idempotência.

## Observe sobrecarga e recuperação

Execute o [laboratório](../README.md), envie o lote de solicitações e espere o consumidor liberar espaço. Compare o `id` das respostas aceitas com o log. Os [testes](../../tests/BoundedJobs.Tests/JobContracts.cs) bloqueiam o primeiro processamento por um sinal explícito, preenchem os outros dois espaços e confirmam o `503`. Depois liberam o processador, validam a ordem e verificam que uma falha não impede o próximo trabalho.

[Documentação de Channels](https://learn.microsoft.com/pt-br/dotnet/core/extensions/channels)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
