# Como saber se uma API ASP.NET Core está viva e pronta para receber tráfego

[English](ARTICLE.en.md) · [Español](ARTICLE.es.md)

Durante um deploy, a API inicia normalmente, mas o serviço de estoque demora para voltar. Se uma única rota de saúde reprovar por causa dessa espera, um orquestrador pode derrubar instâncias que conseguiriam se recuperar sozinhas. Se a rota só confirmar que o processo abriu a porta, a aplicação pode receber tráfego cedo demais. Use sondas diferentes para que cada pergunta tenha uma consequência precisa.

## Decida quem consome cada sinal

| Sinal | Pergunta | Ação típica |
| --- | --- | --- |
| Liveness | O processo consegue responder? | O orquestrador pode reiniciar uma instância que falha repetidamente |
| Readiness | A instância consegue atender agora? | O balanceador retira a instância do tráfego até ela voltar |

No [laboratório de logging](../../0001_serilog_seq_logging/article/ARTICLE.md), usamos eventos para investigar um incidente. Os health checks têm outro consumidor. O endpoint informa um estado curto para a automação. Os logs ajudam a explicar por que a dependência falhou.

## Configure verificações distintas na mesma aplicação

Em [Program.cs](../src/HealthChecks.Api/Program.cs), a liveness usa `Predicate = _ => false`. Ela atende a requisição sem executar os checks registrados. A readiness filtra a tag `ready`.

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

## Reproduza a falha e leia o que aconteceu

Suba o Compose pelo [guia do laboratório](../README.md). Compare os dois endpoints antes e depois de `docker compose stop dependency`. Reinicie a dependência e consulte a readiness até obter `200`. Durante o intervalo, a API permanece viva. Nos [testes](../../tests/HealthChecks.Tests), o estado HTTP da dependência é controlado para validar o contrato sem esperar por um Nginx real.

O [manifesto Kubernetes](../kubernetes.yml) permite ver a consequência fora da API. Escale a dependência para zero e consulte os EndpointSlices. A prontidão do Pod muda e o Service deixa de enviar tráfego àquele endpoint. Quando a dependência volta, compare o contador de reinícios para distinguir recuperação de reinicialização.

## Escolha uma checagem que possa mudar a decisão

Coloque na readiness os serviços indispensáveis ao atendimento. Se uma funcionalidade opcional puder ficar degradada sem impedir as rotas essenciais, acompanhe-a separadamente. Use consultas baratas com timeout e mantenha o diagnóstico detalhado nos logs. Para a próxima etapa do contrato de atendimento, veja [como devolver erros explícitos em Problem Details](../../0004_problem_details/article/ARTICLE.md).

[Health checks no ASP.NET Core](https://learn.microsoft.com/pt-br/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0) · [Sondas no Kubernetes](https://kubernetes.io/docs/concepts/workloads/pods/probes/)

---

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
