# Liveness e readiness no ASP.NET Core

Quando uma API depende de outro serviço, as duas perguntas operacionais são diferentes. O processo ainda responde? Ele consegue receber tráfego e concluir seu trabalho agora? Separar as verificações permite retirar uma instância do roteamento durante uma falha transitória sem reiniciá-la por causa de uma dependência externa.

## Crie duas verificações com os Health Checks

O ASP.NET Core registra os testes com `AddHealthChecks`. Neste laboratório, `DependencyHealthCheck` consulta um endpoint HTTP interno, que é servido por um container Nginx. A tag `ready` identifica a verificação que a API precisa executar antes de receber tráfego.

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

A liveness responde assim que a aplicação consegue processar a requisição. A readiness usa um `HttpClient` com limite de 2 segundos e desabilita redirecionamentos. Uma resposta `2xx` indica que a dependência está acessível. Se a chamada falhar ou retornar outro status, a verificação fica `Unhealthy` e o middleware responde `503`.

## Observe a falha no Docker

Siga as [instruções de execução](../README.md) e interrompa o serviço `dependency` com `docker compose stop dependency`. Em seguida, consulte os dois endpoints. Inicie o serviço novamente para observar a recuperação.

| Estado da dependência | `/health/live` | `/health/ready` |
| --- | --- | --- |
| Disponível | `200 Healthy` | `200 Healthy` |
| Parada | `200 Healthy` | `503 Unhealthy` |
| Recuperada | `200 Healthy` | `200 Healthy` |

O `healthcheck` do Docker Compose usa `/health/live`. Em um orquestrador com sondas distintas, use a liveness para detectar quando o processo precisa ser reiniciado e a readiness para orientar o roteamento. Um Kubernetes Service, por exemplo, retira do tráfego um Pod cuja sonda de readiness falhou. [Documentação de probes do Kubernetes](https://kubernetes.io/docs/concepts/workloads/pods/probes/).

## Escolha o que entra na readiness

Inclua dependências indispensáveis ao atendimento da instância. Consultas devem ser curtas e baratas, pois o orquestrador as executa repetidamente. Os endpoints do exemplo retornam só o estado agregado. Mantenha detalhes internos no logging e configure limites de acesso conforme a rede da implantação. [Health checks no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0).
