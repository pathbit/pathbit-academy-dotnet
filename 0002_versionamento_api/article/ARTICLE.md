# Versionamento de API no ASP.NET Core

Uma mudança de rota ou de tipo no JSON pode quebrar aplicações que já usam uma API. Neste laboratório, dois contratos convivem no mesmo serviço. O cliente escolhe a versão no caminho da requisição, e o Swagger mostra as operações disponíveis em cada uma.

## Defina o contrato de cada versão

Em `Program.cs`, `AddApiVersioning` registra o versionamento. O atributo `[ApiVersion]` declara as versões atendidas pelo controller. O trecho `v{version:apiVersion}` da rota lê a versão informada no caminho. Quando uma ação implementa um contrato específico, `[MapToApiVersion]` liga a ação à versão correspondente.

O `UserController` muda a rota de busca por id de `/api/v1/users/{id}` para `/api/v2/users/id/{id}`. No `CustomerController`, a v1 devolve `document` como string e a v2 como número. Essa é uma quebra de contrato deliberada para o laboratório. Identificadores que podem ter zeros à esquerda devem conservar a representação textual.

## Rode os dois contratos

Com o SDK do .NET 10 instalado, rode os comandos a partir da pasta do capítulo.

```bash
dotnet run --project src/Api --launch-profile http
```

Em outro terminal, compare os tipos e os cabeçalhos.

```bash
curl -i http://localhost:7000/api/v1/customers/1
curl -i http://localhost:7000/api/v2/customers/1
curl -i http://localhost:7000/api/v3/users
```

As duas primeiras respostas são `200 OK`. A v1 informa o documento como `"123.456.789-00"`, enquanto a v2 o representa como `12345678900`. A consulta à v3 retorna `400` com `UnsupportedApiVersion` nesta implementação. Em rotas versionadas, `ReportApiVersions` expõe a v2 em `api-supported-versions` e a v1 em `api-deprecated-versions`.

## Publique uma descrição por versão

Abra o [Swagger local](http://localhost:7000/swagger) em ambiente `Development` e alterne entre `V1` e `V2`. `AddVersionedApiExplorer` agrupa as ações por versão e substitui o parâmetro na URL. `ConfigureSwaggerOptions` registra um documento para cada grupo. O documento correspondente mostra a rota de usuário por id e o tipo de `document` daquele contrato.

A depreciação mantém a v1 acessível e sinaliza a migração. Para planejar a remoção, informe aos clientes quais operações mudam, publique um prazo e verifique quem ainda consome a v1 antes de desligá-la.

O exemplo usa `Microsoft.AspNetCore.Mvc.Versioning` 5.1 para preservar o comportamento original do laboratório. Para novas APIs, a biblioteca é mantida como `Asp.Versioning.Mvc`. [Guia de migração do projeto API Versioning](https://github.com/dotnet/aspnet-api-versioning/wiki/Migration).

[Instruções e tabela de rotas do exemplo](../README.md)
