Se você ainda usa `Console.WriteLine` ou grava logs em texto puro na sua aplicação .NET, você não tem observabilidade: tem apenas poluição visual no terminal.

Quando um erro acontece em produção às 2h da manhã, caçar uma exceção em gigabytes de texto não estruturado com grep ou regex é um pesadelo que custa horas de indisponibilidade.

A solução definitiva no ecossistema .NET moderno é o Logging Estruturado com Serilog e Seq.

🔍 O que muda quando você adota Logging Estruturado?

Em vez de concatenar texto:
❌ `_logger.LogInformation($"Cliente {id} consultou o CEP {cep}");` (vira uma string opaca)

Você captura propriedades nomeadas como cidadãos de primeira classe:
✅ `_logger.LogInformation("Cliente {ClienteId} consultou o CEP {Cep}", id, cep);`

O Serilog preserva os valores originais como objetos JSON tipados. E quando enviados para o Seq, você pode executar consultas poderosas em segundos:
`Cep = '01001000' and StatusCode >= 400 and Elapsed > 500`

🏗️ A arquitetura implementada no módulo 0001 do Pathbit Academy .NET:

1. ASP.NET Core 9.0 + Serilog:
Configuração moderna e desacoplada em métodos de extensão (`SerilogExtensions`), mantendo o `Program.cs` limpo e focado no pipeline da aplicação.

2. Enriquecimento Automático (Context Enrichment):
Toda linha de log recebe automaticamente propriedades operacionais sem esforço manual: `Environment`, `ApplicationName`, `MachineName`, `ThreadId` e dados da requisição HTTP (`UserAgent`, `RequestPath`, `Elapsed`).

3. Centralização em Tempo Real com Seq:
Stack completa em Docker Compose subindo o dashboard do Seq na porta 5341. Visualize gráficos de tráfego, histogramas de latência e traces de erro em tempo real.

4. Resiliência de Ingestão:
Suporte tanto para sink HTTP direto durante o desenvolvimento local (`dotnet run`) quanto via driver de logs em containers.

Disponibilizamos a solução completa em C# (.NET 9), manifesto Docker Compose, arquivo `.http` para testar no VS Code e guia completo de troubleshooting:

🔗 Repositório oficial: https://github.com/pathbit/pathbit-academy-dotnet
📖 Módulo: 0001_serilog_seq_logging

#DotNet #CSharp #Serilog #Seq #Observabilidade #ASPNETCore #DevOps #EngenhariaDeSoftware #PathbitAcademy
