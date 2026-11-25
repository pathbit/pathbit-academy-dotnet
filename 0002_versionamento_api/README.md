# Versionamento de API no ASP.NET Core

Projeto de exemplo que mostra como manter duas versões de uma API no mesmo
serviço: a **v1**, marcada como deprecada, e a **v2**, que traz mudanças que
quebram contrato.

[Leia o artigo](article/ARTICLE.md)

## Pré-requisitos

- SDK do .NET 10. O `global.json` da solução seleciona a faixa instalada mais recente do SDK 10.0.

## Como executar

```bash
cd 0002_versionamento_api
dotnet run --project src/Api --launch-profile http
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

As respostas das rotas versionadas informam as versões disponíveis nos headers (`ReportApiVersions = true`):

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
.NET 10 e preserva os contratos deste exemplo. Em projetos novos, use o sucessor
`Asp.Versioning.Mvc` (mesmos conceitos, namespace `Asp.Versioning`).

---

**Autor:** Eliel Sousa - _Pathbit Academy .NET_
