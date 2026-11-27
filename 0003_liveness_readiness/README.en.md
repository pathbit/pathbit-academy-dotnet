# ASP.NET Core liveness and readiness

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

The sample exposes a process health signal and a separate HTTP dependency check.

[Read the article](article/ARTICLE.en.md)

## Run with Docker Compose

Run these commands from this module. Both endpoints respond `200` once the dependency is available. Use `HEALTH_LAB_PORT` if the sample port is occupied, and use the updated port in curl requests.

```bash
docker compose up -d --build --wait
curl -i http://localhost:19703/health/live
curl -i http://localhost:19703/health/ready
```

## Simulate a dependency outage

Liveness stays at `200` and readiness returns `503`. After starting the dependency, repeat the readiness request until it returns `200`. Stop the lab with `docker compose down`.

```bash
docker compose stop dependency
curl -i http://localhost:19703/health/live
curl -i http://localhost:19703/health/ready
docker compose start dependency
curl -i http://localhost:19703/health/ready
```

## Observe the probes in Kubernetes

Run with Docker, kind, and kubectl. The Service endpoint loses readiness while the dependency is unavailable. Check the restart count before and after recovery.

```bash
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

## Run the API only

Configure `Dependency__HealthUrl` with an HTTP endpoint. Without a service at the given address, liveness returns `200` and readiness returns `503`.

```bash
Dependency__HealthUrl=http://127.0.0.1:19704/ \
  dotnet run --project src/HealthChecks.Api -- --urls http://127.0.0.1:19703
```

Run `dotnet test tests/HealthChecks.Tests` from the repository root with the .NET 10 SDK.

[LinkedIn article](article/ARTICLE_LINKEDIN.en.md) · [LinkedIn post](article/POST_LINKEDIN.en.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
