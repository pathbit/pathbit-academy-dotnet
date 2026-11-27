# Liveness e readiness no ASP.NET Core

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

Uma API mínima expõe dois health checks. O laboratório sobe a API e uma dependência HTTP local via Docker Compose e permite simular uma indisponibilidade.

[Leia o artigo](article/ARTICLE.md)

## Execute com Docker Compose

A partir desta pasta, suba a API e a dependência.

```bash
docker compose up -d --build --wait
curl -i http://localhost:19703/health/live
curl -i http://localhost:19703/health/ready
```

As duas consultas retornam `200 OK` depois que a dependência começa a responder. Defina `HEALTH_LAB_PORT` ao executar o Compose caso a porta `19703` já esteja ocupada.

## Simule falha e recuperação

```bash
docker compose stop dependency
curl -i http://localhost:19703/health/live
curl -i http://localhost:19703/health/ready
docker compose start dependency
curl -i http://localhost:19703/health/ready
```

Durante a falha, `/health/live` permanece em `200` e `/health/ready` retorna `503 Service Unavailable`. A última requisição volta a responder `200` quando o Nginx se recupera. Repita a consulta se a dependência ainda estiver iniciando.

Encerre o laboratório com `docker compose down`. Para rodar os testes, use `dotnet test tests/HealthChecks.Tests` na raiz do repositório com o SDK do .NET 10.

## Valide as sondas com Kubernetes

O [manifesto](kubernetes.yml) configura sondas separadas e um Service para a API. Com Docker, kind e kubectl instalados, execute nesta pasta.

```sh
lab_kubeconfig="$(mktemp -d)/kubeconfig"
kind create cluster --name pathbit-health-lab --kubeconfig "$lab_kubeconfig"
docker build --provenance=false -t pathbit-health-api:local .
kind load docker-image --name pathbit-health-lab pathbit-health-api:local
kubectl --kubeconfig "$lab_kubeconfig" apply -f kubernetes.yml
kubectl --kubeconfig "$lab_kubeconfig" rollout status deployment/health-api
kubectl --kubeconfig "$lab_kubeconfig" scale deployment/dependency --replicas=0
kubectl --kubeconfig "$lab_kubeconfig" wait --for=condition=Ready=false pod -l app=health-api --timeout=60s
kubectl --kubeconfig "$lab_kubeconfig" get endpointslices -l kubernetes.io/service-name=health-api -o yaml
kubectl --kubeconfig "$lab_kubeconfig" scale deployment/dependency --replicas=1
kubectl --kubeconfig "$lab_kubeconfig" wait --for=condition=Ready pod -l app=health-api --timeout=60s
kind delete cluster --name pathbit-health-lab --kubeconfig "$lab_kubeconfig"
```

Durante a falha, o EndpointSlice marca o Pod como `ready: false`. Depois que a dependência volta, o mesmo Pod atende novamente pelo Service. Confira o contador de reinícios com `kubectl get pods` usando o kubeconfig do laboratório.

## Execute apenas a API

A variável `Dependency__HealthUrl` deve apontar para um endpoint HTTP acessível. O valor padrão é `http://127.0.0.1:19704/`. Sem um serviço nesse endereço, a API fica viva e a readiness responde `503`.

```bash
Dependency__HealthUrl=http://127.0.0.1:19704/ \
  dotnet run --project src/HealthChecks.Api -- --urls http://127.0.0.1:19703
```

[Artigo para LinkedIn](article/ARTICLE_LINKEDIN.md) · [Post para LinkedIn](article/POST_LINKEDIN.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
