# ViaCEP Logger API - Serilog + Seq Demo

## 📌 Sobre o Projeto

API .NET que demonstra **logging estruturado** usando **Serilog** e **Seq**.
A aplicação consome a API pública do ViaCEP para consultar endereços por CEP, registrando logs estruturados de todas as operações.### 🎯 Funcionalidades

- ✅ Consulta de CEP via API ViaCEP
- ✅ Logging estruturado com Serilog
- ✅ Visualização de logs com Seq
- ✅ Health check endpoint
- ✅ Swagger UI integrado
- ✅ Redirecionamento automático da raiz para documentação

---

## 🚀 Como Executar

### 📋 Pré-requisitos

- .NET 9.0 SDK ou superior
- Docker e Docker Compose (para o Seq)
- VS Code com extensão REST Client (opcional, para usar arquivos .http)

### 🐳 1. Subir o stack de observabilidade

```bash
# Entrar na pasta do projeto
cd 0001_serilog_seq_logging

# Subir Seq + sidecar GELF + API
docker compose up -d seq seq-gelf api

# Conferir os serviços
docker compose ps
```

- **Seq (UI + ingestão HTTP)**: <http://localhost:5341>
- **Sidecar GELF**: escuta em `udp://localhost:12201` (`pb-seq-gelf`)

Depois de gerar requisições, confira no Seq se os eventos chegam com o `tag` `viaceploggerapi`. Se mudar variáveis/ports, recrie os containers (`docker compose down && docker compose up -d seq seq-gelf api`).

> Dica: ao executar `dotnet run`, o Serilog ativa o sink HTTP automaticamente (`Seq__UseHttpIngestion=true`), então o sidecar só é necessário dentro de containers.


### 🚀 2. Executar a API

```bash
# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run --project src/ViaCepLogger.Api/ViaCepLogger.Api.csproj
```

**API estará disponível em:**

- 🌐 **HTTPS:** <https://localhost:7001>
- 🌐 **HTTP:** <http://localhost:5001>
- 📖 **Swagger:** <https://localhost:7001/> (redirecionamento automático)

---

## 🧪 Testando a API

### 🎯 Opção 1: Arquivo .http (Recomendado)

1. Abra `src/ViaCepLogger.Api/ViaCepLogger.Api.http` no VS Code
2. Clique em **"Send Request"** acima de cada endpoint
3. Veja as respostas em tempo real

### 🎯 Opção 2: cURL

```bash
# Health check
curl http://localhost:5001/api/cep/health

# CEP válido (São Paulo - Centro)
curl http://localhost:5001/api/cep/01001000

# CEP inválido (formato válido, mas não existe)
curl http://localhost:5001/api/cep/00000000

# CEP malformado (deve retornar 400)
curl http://localhost:5001/api/cep/abc123
```

### 🎯 Opção 3: Swagger UI

1. Acesse: <https://localhost:7001/>
2. Teste os endpoints diretamente na interface

---

## 📊 Visualizando os Logs

### 🔍 No Seq Dashboard

1. **Acesse:** <http://localhost:5341>
2. **Execute** alguns requests na API
3. **Observe** os logs estruturados em tempo real
4. **Use filtros:**
   - `StatusCode = 404` - CEPs não encontrados
   - `@Level = 'Error'` - Apenas erros
   - `RequestPath like '/api/cep%'` - Apenas requests de CEP

### 🔍 No Terminal

Logs também são exibidos no console com formatação colorida e estruturada.

---

## 🛠️ Tecnologias Utilizadas

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| .NET | 9.0 | Framework principal |
| Serilog | 8.0.3 | Logging estruturado |
| Seq | - | Dashboard de logs |
| Swagger/OpenAPI | - | Documentação da API |
| ViaCEP API | - | Fonte de dados de CEP |

---

## 🔍 Estrutura do Projeto

```text
src/
├── docker-compose.yml              # Configuração do Seq
├── ViaCepLogger.Api/
│   ├── ViaCepLogger.Api.http      # Testes HTTP (VS Code)
│   ├── Program.cs                 # Configuração da aplicação
│   ├── Usings.cs                  # Global usings do projeto
│   ├── appsettings.json          # Configurações
│   ├── Controllers/
│   │   └── CepController.cs      # Endpoints da API
│   ├── Services/
│   │   └── ViaCepService.cs      # Integração com ViaCEP
│   ├── Models/
│   │   └── ViaCepResponse.cs     # Modelo de dados
│   └── Infrastructure/
│       └── Converters/
│           └── StringToBoolConverter.cs  # Converters customizados
```

---

## 🎯 Endpoints Disponíveis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/` | Redireciona para Swagger UI |
| `GET` | `/api/cep/health` | Health check da aplicação |
| `GET` | `/api/cep/{cep}` | Consulta CEP via ViaCEP |

### 📝 Exemplos de Resposta

**✅ CEP Válido (200):**

```json
{
  "cep": "01001-000",
  "logradouro": "Praça da Sé",
  "bairro": "Sé",
  "localidade": "São Paulo",
  "uf": "SP"
}
```

**❌ CEP Não Encontrado (404):**

```json
{
  "message": "CEP não encontrado"
}
```

**❌ CEP Inválido (400):**

```json
{
  "message": "CEP deve conter exatamente 8 dígitos"
}
```

---

## 💡 Sobre Logging Estruturado

Este projeto demonstra as vantagens do logging estruturado sobre logs tradicionais:

- ✅ **Pesquisável:** Filtros por campos específicos
- ✅ **Contextual:** Informações estruturadas sobre cada operação
- ✅ **Correlacionável:** Request ID para rastrear operações
- ✅ **Métricas:** Análise de performance e padrões
- ✅ **Alertas:** Configuração de alertas baseados em condições

---

**Autor:** Eliel Sousa - _Pathbit Academy .NET_
