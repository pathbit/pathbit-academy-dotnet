# Como armazenar logs de forma eficiente no .NET com Serilog e Seq

**Vamos dar um contexto no tema:**

Se você ainda está usando `Console.WriteLine` para debugar sua aplicação .NET, ou pior, se está usando `Debug.WriteLine` achando que está fazendo logging, precisa parar agora e ler este artigo. Não é sobre ser moderno ou seguir hype, é sobre não perder horas (ou dias) tentando entender o que deu errado em produção.

Logging estruturado não é novidade, mas a quantidade de projetos que ainda fazem isso errado é impressionante. E quando digo errado, não é só sobre não ter logs, é sobre ter logs que não servem para nada quando você realmente precisa deles.

> Vale conferir este panorama sobre logging estruturado na prática: [Structured Logging for .NET Developers](https://www.hanselman.com/blog/structured-logging-in-aspnet-core-with-serilog-and-seq).

## O problema que ninguém quer admitir

Você já passou por isso: algo quebrou em produção, você corre para ver os logs e encontra mensagens genéricas tipo "Erro ao processar", "Falha na requisição" ou o clássico "Algo deu errado". Parabéns, você tem logs inúteis.

O problema não é a falta de logs, é a falta de **contexto**. Quando você escreve `Console.WriteLine($"Erro: {ex.Message}")`, está jogando informação no vazio. Não tem timestamp estruturado, não tem nível de severidade, não tem contexto da requisição, não tem nada que te ajude a entender o que realmente aconteceu.

E aí você faz o quê? Adiciona mais `Console.WriteLine` tentando adivinhar onde está o problema, faz deploy, espera o erro acontecer de novo, e repete o ciclo. É ineficiente, frustrante e caro.

## Por que Serilog e Seq?

Existem várias opções de logging no ecossistema .NET: ILogger nativo, NLog, log4net, e por aí vai. Então por que Serilog e Seq?

**Serilog** é uma biblioteca de logging estruturado que trata logs como dados, não como texto. Cada log é um evento com propriedades tipadas que você pode consultar, filtrar e analisar. É simples de configurar, tem performance excelente e se integra perfeitamente com o ecossistema .NET.

**Seq** é uma ferramenta de visualização e análise de logs estruturados. Pense nele como um "banco de dados de logs" com uma interface web poderosa para consultas. A versão gratuita permite até 50GB de logs por mês, o que é mais do que suficiente para projetos pequenos e médios.

A combinação dos dois resolve o problema de forma elegante: você escreve logs estruturados com Serilog e visualiza/analisa com Seq. Simples assim.

## O que é logging estruturado na prática?

Logging estruturado significa tratar logs como eventos com propriedades, não como strings concatenadas.

**Jeito errado (logging tradicional):**

```csharp
Console.WriteLine($"Usuário {userId} acessou o recurso {resourceId} em {DateTime.Now}");
```

**Jeito certo (logging estruturado):**

```csharp
_logger.LogInformation("Usuário {UserId} acessou o recurso {ResourceId}", userId, resourceId);
```

A diferença parece sutil, mas é fundamental. No segundo caso, `UserId` e `ResourceId` são propriedades estruturadas que você pode consultar no Seq com queries como `UserId = 123` ou `ResourceId = "abc"`. No primeiro caso, é só uma string que você teria que fazer regex ou parsing manual para extrair informação.

## Quando usar Serilog e Seq?

> Spoiler pro time de operações: log sem contexto é igual foto borrada do incidente, ninguém entende nada.

**Use quando:**

- Você precisa entender o que está acontecendo na sua aplicação em produção
- Você quer troubleshooting rápido sem ficar adicionando logs e fazendo redeploy
- Você tem múltiplos serviços e precisa correlacionar logs entre eles
- Você quer métricas e análises sobre o comportamento da aplicação
- Você quer alertas baseados em padrões de logs

**Não use quando:**

- Você está fazendo um script de 50 linhas que roda uma vez por mês
- Você tem requisitos de compliance que exigem soluções específicas
- Você já tem uma stack de observabilidade consolidada (ELK, Datadog, etc.)

Para projetos pequenos e médios, Serilog + Seq é o sweet spot entre simplicidade e poder. Para projetos grandes, você provavelmente vai querer algo mais robusto como ELK Stack ou soluções comerciais, mas mesmo assim, Serilog continua sendo uma excelente escolha como biblioteca de logging.

## Limitações do Seq gratuito

O Seq tem uma versão gratuita com limite de **50GB de logs por mês**. Parece pouco? Vamos fazer as contas:

- 50GB = 50.000 MB
- Assumindo 1KB por evento de log (o que é bastante)
- 50.000 MB / 1KB = 50 milhões de eventos por mês
- Isso dá aproximadamente 1,6 milhões de eventos por dia
- Ou 19 eventos por segundo, 24/7

Para a maioria dos projetos pequenos e médios, isso é mais do que suficiente. Se você está ultrapassando esse limite, provavelmente já tem budget para a versão paga ou para uma solução enterprise.

**Dicas para não estourar o limite:**

- Use níveis de log apropriados (não logue tudo como Information)
- Configure retention policies (não precisa manter logs de debug por 30 dias)
- Use sampling em ambientes de alta carga (logue 1 a cada N requisições)
- Filtre logs desnecessários antes de enviar para o Seq

## Exemplo prático: API que consome ViaCEP

Vamos criar uma API real que consome o serviço ViaCEP e registra logs estruturados para diferentes cenários HTTP. Isso vai mostrar como o Serilog e Seq funcionam na prática, não em exemplos de tutorial.

**Cenários que vamos cobrir:**

- Requisição bem-sucedida (200)
- CEP não encontrado (404)
- Erro no serviço externo (500)
- Timeout de requisição
- Formato de CEP inválido (400)

### Estrutura do projeto

```bash
src/
├── ViaCepLogger.sln
├── docker-compose.yml
└── ViaCepLogger.Api/
    ├── ViaCepLogger.Api.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── Controllers/
    │   └── CepController.cs
    ├── Services/
    │   └── ViaCepService.cs
    └── Models/
        └── ViaCepResponse.cs
```

### Configuração do Serilog

No `Program.cs` só precisamos chamar a extensão que centraliza toda a configuração padrão:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultSerilog();

var app = builder.Build();

app.UseDefaultSerilogRequestLogging();
```

Uma única extensão em `Extensions/SerilogExtensions.cs` cuida tanto da configuração do host quanto do middleware de request logging. Ela aplica enrichers, escolhe sinks/formatter e configura os dados adicionais da requisição via `EnrichDiagnosticContext`:

```csharp
const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Application} {Environment} {MachineName} {ThreadId} {Message:lj}{NewLine}{Exception}";

var isRunningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

var configuredBridge = seqSection.GetValue<bool?>("UseAgentBridge");
var configuredHttp = seqSection.GetValue<bool?>("UseHttpIngestion");

var useAgentBridge = isRunningInContainer
    ? (configuredBridge ?? true)
    : (configuredBridge ?? false);

var useHttpIngestion = isRunningInContainer
    ? (configuredHttp ?? false)
    : (configuredHttp ?? true);

configuration
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", applicationName)
    .WriteTo.Async(sink =>
    {
        if (useAgentBridge)
            sink.Console(new RenderedCompactJsonFormatter());
        else
            sink.Console(outputTemplate: outputTemplate);
    });

if (useHttpIngestion)
{
    configuration.WriteTo.Async(sink =>
    {
        if (!string.IsNullOrWhiteSpace(seqApiKey))
            sink.Seq(serverUrl: seqServerUrl, apiKey: seqApiKey);
        else
            sink.Seq(serverUrl: seqServerUrl);
    });
}

options.EnrichDiagnosticContext = (ctx, httpContext) =>
{
    ctx.Set("RequestScheme", httpContext.Request.Scheme);
    ctx.Set("RequestHost", httpContext.Request.Host.Value ?? "Unknown");
    ctx.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown");
};
```

**O que isso entrega na prática:**

- O nome da aplicação (`Seq:ApplicationName`) vira uma propriedade fixa (`Application`) em todos os eventos.
- Fora de containers o sink HTTP (`Serilog.Sinks.Seq`) fica ligado por padrão, enviando tudo direto para o Seq.
- Dentro de containers, a variável `DOTNET_RUNNING_IN_CONTAINER` ativa o bridge por padrão; no compose reforçamos `Seq__UseAgentBridge=true`/`Seq__UseHttpIngestion=false` para priorizar a saída estruturada no stdout.
- Tudo continua assíncrono (`Serilog.Sinks.Async`), então logging não bloqueia requisições.

#### Configuração via appsettings

```json
"Seq": {
  "ServerUrl": "http://localhost:5341",
  "UseHttpIngestion": true,
  "UseAgentBridge": false,
  "ApplicationName": "ViaCepLogger.Api"
}
```

Em ambientes containerizados sobrescrevemos com variáveis (`Seq__UseAgentBridge=true`, `Seq__UseHttpIngestion=false`) para priorizar a saída compacta no stdout. Como a demo deixa a ingestão sem chave (`SEQ_REQUIRE_INGESTION_API_KEY=false`), `Seq__ApiKey` pode ficar vazio; se você exigir chave, basta definir o valor via variável.

#### Pacotes que fazem a mágica acontecer

- `Serilog.AspNetCore`: integra Serilog ao pipeline do ASP.NET Core e expõe `UseSerilog()`.
- `Serilog.Enrichers.Environment` e `Serilog.Enrichers.Thread`: enriquecem com ambiente, máquina e thread automaticamente.
- `Serilog.Sinks.Console`: exibe os logs com o template padrão fora do bridge.
- `Serilog.Formatting.Compact`: fornece o `RenderedCompactJsonFormatter` usado pelo bridge.
- `Serilog.Sinks.Seq`: envia eventos via HTTP quando habilitado.
- `Serilog.Sinks.Async`: mantém os sinks desacoplados da thread da requisição.

> Sem o buffer assíncrono, cada `Log.Information` aguardaria o console ou Seq finalizar a escrita; com `WriteTo.Async(...)`, o fluxo segue sem travar I/O.

### Implementação do serviço

O `ViaCepService` é onde a mágica acontece. Vamos logar cada cenário de forma estruturada:

```csharp
public async Task<ViaCepResponse?> GetAddressByCepAsync(string cep)
{
    try
    {
        _logger.LogInformation("Iniciando consulta de CEP {Cep}", cep);

        var response = await _httpClient.GetAsync($"https://viacep.com.br/ws/{cep}/json/");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("CEP {Cep} não encontrado (404)", cep);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Erro ao consultar CEP {Cep}. Status: {StatusCode}",
                cep,
                response.StatusCode
            );
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var address = JsonSerializer.Deserialize<ViaCepResponse>(content);

        _logger.LogInformation(
            "CEP {Cep} consultado com sucesso. Cidade: {Cidade}, UF: {UF}",
            cep,
            address?.Localidade,
            address?.Uf
        );

        return address;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(
            ex,
            "Erro de rede ao consultar CEP {Cep}",
            cep
        );
        return null;
    }
    catch (TaskCanceledException ex)
    {
        _logger.LogError(
            ex,
            "Timeout ao consultar CEP {Cep}",
            cep
        );
        return null;
    }
}
```

**Pontos importantes:**

- Cada cenário tem seu nível de log apropriado (Information, Warning, Error)
- Propriedades estruturadas (`{Cep}`, `{StatusCode}`) podem ser consultadas no Seq
- Exceções são logadas com contexto completo

### Visualizando no Seq

Depois de executar algumas requisições, você pode usar o Seq para análises poderosas:

![Dashboard do Seq mostrando logs estruturados](https://raw.githubusercontent.com/pathbit/pathbit-academy-dotnet/master/0001_serilog_seq_logging/assets/01.png)
*Figura 1: Interface principal do Seq com eventos de log da aplicação ViaCepLogger*

**Consultas úteis:**

```bash
# Todos os erros
@Level = 'Error'

# CEPs não encontrados
StatusCode = 404

# Requisições lentas (mais de 1 segundo)
@Duration > 1000

# Erros de um CEP específico
Cep = '01001000' and @Level = 'Error'

# Agrupamento por status code
select StatusCode, count(*) group by StatusCode
```

![Detalhes de um log estruturado no Seq](https://raw.githubusercontent.com/pathbit/pathbit-academy-dotnet/master/0001_serilog_seq_logging/assets/02.png)
*Figura 2: Propriedades estruturadas de um evento de log*

![Query e filtro de logs no Seq](https://raw.githubusercontent.com/pathbit/pathbit-academy-dotnet/master/0001_serilog_seq_logging/assets/03.png)
*Figura 3: Filtrando logs por StatusCode no Seq*

![Gráfico de agregação de logs](https://raw.githubusercontent.com/pathbit/pathbit-academy-dotnet/master/0001_serilog_seq_logging/assets/04.png)
*Figura 4: Visualização gráfica da distribuição de logs por status code*

O Seq também permite criar dashboards, alertas e exportar dados para análise externa.

### Alertas e notificações com Output Apps

Seq não dispara alerta sozinho: você precisa combinar **signals**, **alerts** e um **output app**. O fluxo básico é:

1. Abra `Settings → Signals` e crie um filtro que represente a condição que você quer monitorar (por exemplo `@Level = 'Error' and SourceContext = 'ViaCepLogger.Api'`).
2. Em `Settings → Alerts`, associe o signal ao alerta e defina limites, frequência e janela de observação.
3. Instale o output app desejado em `Settings → Output apps` (e-mail, Slack, Teams, webhook, etc.). Os apps oficiais estão em [datalust.co/docs/installing-output-apps](https://datalust.co/docs/installing-output-apps) e você pode publicar apps próprios se precisar.

Quando um evento entrar no signal, o alerta dispara e o output app cuida de entregar a notificação. Esse modelo é interessante porque você consegue reaproveitar o mesmo signal para dashboards e alertas diferentes sem duplicar lógica.

### Coletando logs de containers Docker

Quando a aplicação roda em container, mantemos o bridge (`Seq__UseAgentBridge=true`) para emitir JSON compacto no stdout. Em vez de habilitar o input GELF dentro do Seq, usamos o sidecar oficial `datalust/seq-input-gelf`: ele escuta na porta `12201/udp` e encaminha os eventos para o Seq via HTTP (`SEQ_ADDRESS=http://seq:80`).

No compose, o serviço `api` já declara `logging.driver: gelf` e aponta para `udp://127.0.0.1:12201`; do ponto de vista do daemon Docker, o destino é o host que expõe a porta `12201/udp` do sidecar. Se preferir manter HTTP direto, basta remover o serviço `seq-gelf`, trocar as flags para `Seq__UseAgentBridge=false`/`Seq__UseHttpIngestion=true` e excluir a seção `logging`.

### Alternando ingestão conforme o ambiente

Dentro do container o .NET expõe a variável `DOTNET_RUNNING_IN_CONTAINER=true`, então aproveitamos isso para ligar o modo "bridge" automaticamente. No `SerilogExtensions` ficou assim:

```csharp
var isRunningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

var configuredBridge = seqSection.GetValue<bool?>("UseAgentBridge");
var configuredHttp = seqSection.GetValue<bool?>("UseHttpIngestion");

var useAgentBridge = isRunningInContainer
    ? (configuredBridge ?? true)
    : (configuredBridge ?? false);

var useHttpIngestion = isRunningInContainer
    ? (configuredHttp ?? false)
    : (configuredHttp ?? true);
```

Quando `UseAgentBridge` está ativo, os logs vão para o `stdout` em formato compacto JSON (`RenderedCompactJsonFormatter`) para que um driver externo/coletor processe o fluxo. Ao detectar que está dentro de um container, o código assume `UseAgentBridge=true` e `UseHttpIngestion=false` a menos que você sobrescreva explicitamente. Fora do container, o padrão se inverte (HTTP ativo, bridge desativado). Se quiser forçar qualquer combinação, basta definir as flags via variável de ambiente (`Seq__UseAgentBridge` / `Seq__UseHttpIngestion`).

No compose de exemplo optamos pelo driver GELF, portanto mantemos o bridge e desativamos o sink HTTP. Como o Seq está configurado para não exigir chave (`SEQ_REQUIRE_INGESTION_API_KEY=false`), `Seq__ApiKey` permanece em branco.

Para reforçar a identificação da origem, adicionamos `Seq:ApplicationName` nas configurações — o `SerilogExtensions` enriquece cada evento com a propriedade `Application`, então no Seq fica claro se veio da API, de um worker ou de outro serviço.

Se decidir exigir API key, gere uma chave no Seq e atribua em `Seq__ApiKey`; o driver GELF continuará funcionando porque ignora esse valor.

O serviço da API ficou assim no `docker-compose.yml`:

```yaml
  seq-gelf:
    image: datalust/seq-input-gelf:latest
    depends_on:
      - seq
    environment:
      - SEQ_ADDRESS=http://seq:80
      - SEQ_API_KEY=${SEQ_INGESTION_API_KEY:-}
    ports:
      - "12201:12201/udp"
    restart: unless-stopped

  api:
    build:
      context: .
      dockerfile: src/ViaCepLogger.Api/Dockerfile
    environment:
      - Seq__UseAgentBridge=true
      - Seq__UseHttpIngestion=false
      - Seq__ApplicationName=ViaCepLogger.Api
    logging:
      driver: gelf
      options:
        gelf-address: udp://127.0.0.1:12201
        tag: via-cep-api
```

O `DOTNET_RUNNING_IN_CONTAINER` fica `true` nesse cenário, e as variáveis reforçam o modo bridge para alimentar o driver GELF.

![Log de erro com stack trace completo](https://raw.githubusercontent.com/pathbit/pathbit-academy-dotnet/master/0001_serilog_seq_logging/assets/08.png)
*Figura 8: Detalhes de um erro no Seq incluindo stack trace e contexto*

## Boas práticas de logging

### 1. Use níveis de log apropriados

- **Trace**: Informação extremamente detalhada, geralmente desabilitada em produção
- **Debug**: Informação útil para debugging, desabilitada em produção
- **Information**: Eventos importantes do fluxo da aplicação
- **Warning**: Situações anormais que não impedem o funcionamento
- **Error**: Erros que impedem uma operação específica
- **Critical**: Erros que podem derrubar a aplicação

### 2. Não logue informações sensíveis

```csharp
// NUNCA faça isso
_logger.LogInformation("Usuário {Email} fez login com senha {Password}", email, password);

// Faça isso
_logger.LogInformation("Usuário {Email} fez login com sucesso", email);
```

### 3. Use propriedades estruturadas, não interpolação

```csharp
// Errado
_logger.LogInformation($"Processando pedido {orderId}");

// Certo
_logger.LogInformation("Processando pedido {OrderId}", orderId);
```

### 4. Adicione contexto com scopes

```csharp
_logger.LogInformation("Processando requisição para usuário {UserId} e request {RequestId}", userId, requestId);
```

### 5. Não exagere nos logs

Mais logs não significa melhor troubleshooting. Logs demais geram ruído e dificultam encontrar o que realmente importa. Logue eventos importantes, não cada linha de código executada.

## Ah pare de falar e `Show-Me-The-Code`

![Logs estruturados no console](https://raw.githubusercontent.com/pathbit/pathbit-academy-dotnet/master/0001_serilog_seq_logging/assets/05.png)
*Figura 5: Logs formatados aparecendo no terminal durante a execução*

![Configuração do Serilog no VS Code](https://raw.githubusercontent.com/pathbit/pathbit-academy-dotnet/master/0001_serilog_seq_logging/assets/06.png)
*Figura 6: Código de configuração do Serilog no Program.cs*

![Diagrama de arquitetura da solução](https://raw.githubusercontent.com/pathbit/pathbit-academy-dotnet/master/0001_serilog_seq_logging/assets/07.png)
*Figura 7: Fluxo de logs da aplicação através do Serilog para Console e Seq*

Todo o código fonte está disponível na pasta `src/` deste artigo e no repositório do GitHub.

**Opção 1:** Baixe o repositório para o seu computador, explore o código e adapte para o seu cenário.

> Abra direto no GitHub e siga o README:

[Abrir repositório no GitHub](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0001_serilog_seq_logging)

**Opção 2:** Rode a stack completa localmente com Docker e .NET CLI para sentir os logs estruturados na prática.

> Passo a passo rápido (executar dentro da pasta `src/`):

```bash
# 1. Subir o Seq
cd src
docker-compose up -d

# 2. Executar a API
dotnet run --project ViaCepLogger.Api

# 3. Testar os endpoints
curl http://localhost:5001/api/cep/01001000  # Sucesso
curl http://localhost:5001/api/cep/00000000  # Não encontrado
curl http://localhost:5001/api/cep/abc123    # Formato inválido

# 4. Acessar o Seq
http://localhost:5341/
```

## Próximos passos

Logging estruturado é a base para observabilidade. Depois de dominar Serilog e Seq, os próximos passos naturais são:

1. **Métricas**: Adicionar Application Insights ou Prometheus para métricas de performance
2. **Tracing distribuído**: Implementar OpenTelemetry para rastrear requisições entre serviços
3. **Alertas**: Configurar alertas no Seq para situações críticas
4. **Dashboards**: Criar dashboards customizados para monitoramento em tempo real

Mas antes de sair correndo para adicionar mais ferramentas, domine o básico. Logging estruturado bem feito resolve 80% dos problemas de troubleshooting. O resto é otimização.

### Então, antes de adicionar mais complexidade, faça o básico

1. **Configure Serilog corretamente**: Níveis de log, enrichers, sinks apropriados
2. **Use propriedades estruturadas**: Sempre. Sem exceção.
3. **Adicione contexto**: Scopes, correlation IDs, informações da requisição
4. **Teste seus logs**: Execute cenários de erro e veja se os logs fazem sentido
5. **Revise periodicamente**: Logs que ninguém usa são desperdício de recursos

### Se quer começar agora, sem complicação

1. **Clone o repositório** e execute o exemplo
2. **Faça algumas requisições** e veja os logs no Seq
3. **Experimente as queries** para entender o poder do logging estruturado
4. **Adapte para seu projeto** começando com os pontos críticos

No final das contas, logging não é sobre ter a ferramenta mais cara ou a stack mais complexa. É sobre ter a informação certa, no momento certo, quando você mais precisa. Serilog e Seq entregam isso de forma simples e eficaz.

Se você entender isso e aplicar no seu projeto, já está anos-luz à frente da maioria que ainda está usando `Console.WriteLine` e rezando para não ter problemas em produção.

## `O básico da observabilidade é logar com estrutura e contexto e não com fé.`

## Referências

- [Serilog - Documentação Oficial](https://serilog.net/)
- [Seq - Documentação Oficial](https://docs.datalust.co/docs)
- [Structured Logging - Best Practices](https://stackify.com/what-is-structured-logging-and-why-developers-need-it/)
- [.NET Logging - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- [ViaCEP - API Pública](https://viacep.com.br/)
