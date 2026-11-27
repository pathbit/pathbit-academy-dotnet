# Liveness y readiness en ASP.NET Core

[Português (Brasil)](README.md) · [English](README.en.md) · [Español](README.es.md)

El ejemplo expone una señal de salud del proceso y una comprobación aparte de una dependencia HTTP.

[Lee el artículo](article/ARTICLE.es.md)

## Ejecuta con Docker Compose

Ejecuta desde esta carpeta. Los dos endpoints devuelven `200` cuando responde la dependencia. Si el puerto está ocupado, configura `HEALTH_LAB_PORT` y ajusta las URLs de curl.

```bash
docker compose up -d --build --wait
curl -i http://localhost:19703/health/live
curl -i http://localhost:19703/health/ready
```

## Simula una caída de la dependencia

Liveness mantiene `200` y readiness devuelve `503`. Después de restaurar la dependencia, repite la consulta de readiness hasta obtener `200`. Cierra el laboratorio con `docker compose down`.

```bash
docker compose stop dependency
curl -i http://localhost:19703/health/live
curl -i http://localhost:19703/health/ready
docker compose start dependency
curl -i http://localhost:19703/health/ready
```

## Observa las sondas en Kubernetes

Usa Docker, kind y kubectl. El endpoint del Service deja de estar listo mientras falla la dependencia. Compara el contador de reinicios antes y después.

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

## Ejecuta solo la API

Configura `Dependency__HealthUrl` con un endpoint HTTP. Si esa dirección no responde, liveness devuelve `200` y readiness devuelve `503`.

```bash
Dependency__HealthUrl=http://127.0.0.1:19704/ \
  dotnet run --project src/HealthChecks.Api -- --urls http://127.0.0.1:19703
```

Ejecuta `dotnet test tests/HealthChecks.Tests` desde la raíz con el SDK de .NET 10.

[Artículo para LinkedIn](article/ARTICLE_LINKEDIN.es.md) · [Publicación para LinkedIn](article/POST_LINKEDIN.es.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
