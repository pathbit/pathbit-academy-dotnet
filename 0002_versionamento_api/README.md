# Versionamento de API no ASP.NET Core

Projeto de exemplo que mostra como manter duas versões de uma API no mesmo
serviço: a **v1**, marcada como deprecada, e a **v2**, que traz mudanças que
quebram contrato.

> Este exemplo ainda não tem artigo publicado. O código é a referência.

## Pré-requisitos

- .NET 9.0 SDK: o `global.json` pede o 9.0.306 e, pela regra padrão (`latestPatch`), aceita patches mais novos da faixa 9.0.3xx

## Como executar

```bash
cd 0002_versionamento_api
dotnet run --project src/Api
```

- API: <http://localhost:7000>
- Swagger UI (ambiente Development): <http://localhost:7000/swagger>, com um documento por versão (`V1` e `V2`)

## Endpoints

| Versão | Método | Rota | Observação |
| :--- | :--- | :--- | :--- |
| v1 (deprecada) | `GET` | `/api/v1/users` | Lista usuários |
| v1 (deprecada) | `GET` | `/api/v1/users/{id}` | Usuário por id |
| v1 (deprecada) | `GET` | `/api/v1/customers/{id}` | `document` como string (`"123.456.789-00"`) |
| v2 | `GET` | `/api/v2/users` | Lista usuários |
| v2 | `GET` | `/api/v2/users/id/{id}` | A rota mudou em relação à v1 |
| v2 | `GET` | `/api/v2/customers/{id}` | `document` como número (`12345678900`) |

Os exemplos prontos estão em [`src/Api/Api.http`](src/Api/Api.http).

## O que observar

Toda resposta informa as versões disponíveis nos headers (`ReportApiVersions = true`):

```http
HTTP/1.1 200 OK
api-supported-versions: 2.0
api-deprecated-versions: 1.0
```

| Requisição | Resultado |
| :--- | :--- |
| `GET /api/v3/users` | `400` com `"code": "UnsupportedApiVersion"` |
| `GET /api/v2/users/1` | `405`: a rota `/users/{id}` só existe na v1 |
| `GET /api/v2/users/id/99` | `404`: usuário inexistente |

## Nota sobre a biblioteca

O projeto usa `Microsoft.AspNetCore.Mvc.Versioning` 5.1.0, que funciona no
.NET 9 mas não recebe mais atualizações. Em projetos novos, use o sucessor
`Asp.Versioning.Mvc` (mesmos conceitos, namespace `Asp.Versioning`).

---

**Autor:** Eliel Sousa - _Pathbit Academy .NET_
