# Quando tirar a API do tráfego sem reiniciar o processo

No exercício, o ASP.NET Core responde `200` em liveness mesmo durante uma falha de uma dependência. A readiness volta a `200` quando a dependência se recupera.

## Decida quem consome cada sinal

| Sinal | Pergunta | Ação típica |
| --- | --- | --- |
| Liveness | O processo consegue responder? | O orquestrador pode reiniciar uma instância que falha repetidamente |
| Readiness | A instância consegue atender agora? | O balanceador retira a instância do tráfego até ela voltar |

No [laboratório de logging](https://github.com/pathbit/pathbit-academy-dotnet/blob/master/0001_serilog_seq_logging/article/ARTICLE.md), usamos eventos para investigar um incidente. Os health checks têm outro consumidor. O endpoint informa um estado curto para a automação. Os logs ajudam a explicar por que a dependência falhou.

## Configure verificações distintas na mesma aplicação

Em [Program.cs](https://github.com/pathbit/pathbit-academy-dotnet/blob/master/0003_liveness_readiness/src/HealthChecks.Api/Program.cs), a liveness usa `Predicate = _ => false`. Ela atende a requisição sem executar os checks registrados. A readiness filtra a tag `ready`.

```csharp
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});
```

A checagem de readiness chama uma dependência HTTP. O cliente usa timeout de dois segundos, desabilita redirects e considera qualquer `2xx` saudável. Se a dependência parar, o middleware expõe `503` para aquela sonda. A liveness continua atendendo.

## Reproduza o cenário no laboratório

Pare o serviço `dependency`, consulte as duas rotas e compare os endpoints do Service no Kubernetes. O resultado mostra por que a decisão de reiniciar e a de rotear precisam de sinais próprios.

[Artigo completo e laboratório no GitHub](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0003_liveness_readiness)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
