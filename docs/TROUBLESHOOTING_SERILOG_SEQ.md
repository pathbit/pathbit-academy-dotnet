# Troubleshooting - Serilog e Seq

## Problema: Timeout ao restaurar pacotes NuGet

**Erro:**
```
error NU1301: Não é possível carregar o índice de serviço para a origem https://api.nuget.org/v3/index.json.
A solicitação HTTP para 'GET https://api.nuget.org/v3/index.json' expirou após 100000ms.
```

**Causas possíveis:**
1. Conexão lenta com a internet
2. Firewall ou proxy bloqueando acesso ao NuGet
3. Servidor do NuGet temporariamente indisponível
4. Muitos pacotes sendo baixados simultaneamente

**Soluções:**

### Solução 1: Aumentar o timeout do NuGet
```bash
# Aumentar timeout para 300 segundos (5 minutos)
dotnet nuget config set http.timeout 300
```

### Solução 2: Limpar cache do NuGet e tentar novamente
```bash
# Limpar cache
dotnet nuget locals all --clear

# Restaurar pacotes
cd pathbit-academy-dotnet/0001_serilog_seq_logging/src
dotnet restore
```

### Solução 3: Usar mirror do NuGet (se disponível)
```bash
# Adicionar source alternativa (exemplo)
dotnet nuget add source https://pkgs.dev.azure.com/_packaging/feed/nuget/v3/index.json -n AzureDevOps
```

### Solução 4: Restaurar pacotes um por um
```bash
cd pathbit-academy-dotnet/0001_serilog_seq_logging/src/ViaCepLogger.Api

# Restaurar pacotes individualmente
dotnet add package Serilog.AspNetCore --version 8.0.3
# Aguardar completar antes de adicionar o próximo

dotnet add package Serilog.Sinks.Seq --version 8.0.0
# Aguardar completar

dotnet add package Serilog.Enrichers.Environment --version 3.1.0
# Aguardar completar

dotnet add package Serilog.Enrichers.Thread --version 4.0.0
# Aguardar completar
```

### Solução 5: Verificar conexão e tentar novamente
```bash
# Testar conectividade com NuGet
curl -I https://api.nuget.org/v3/index.json

# Se funcionar, tentar restore novamente
dotnet restore
```

---

## Outros Problemas Comuns

### Docker não está rodando

**Erro:**

```bash
Cannot connect to the Docker daemon
```

**Solução:**
```bash
# Verificar se Docker está rodando
docker ps

# Iniciar Docker Desktop (Windows/Mac)
# ou iniciar serviço (Linux)
sudo systemctl start docker
```

### Porta 5341 já está em uso

**Erro:**
```
Bind for 0.0.0.0:5341 failed: port is already allocated
```

**Solução:**
```bash
# Parar containers existentes
docker-compose down

# Ou alterar a porta no docker-compose.yml
# Mudar "5341:80" para "5342:80" (ou outra porta disponível)
```

### Erro ao compilar o projeto

**Erro:**
```
error CS0246: The type or namespace name 'Serilog' could not be found
```

**Solução:**
```bash
# Restaurar pacotes
dotnet restore

# Limpar e rebuildar
dotnet clean
dotnet build
```

### API não responde

**Problema:**
A API não responde nas portas esperadas (5001/7001)

**Solução:**
```bash
# Verificar se a API está rodando
ps aux | grep dotnet

# Verificar portas em uso
lsof -i :5001
lsof -i :7001

# Verificar logs da aplicação para erros de inicialização
```

### Seq não mostra logs

**Problema:**
Seq está rodando mas não mostra logs da aplicação

**Verificações:**
1. Confirmar que a API está enviando logs:
   ```bash
   # Verificar logs no console
   # Deve aparecer logs quando fizer requisições
   ```

2. Verificar configuração do Seq no appsettings.json:
   ```json
   "Seq": {
     "ServerUrl": "http://localhost:5341"
   }
   ```

3. Verificar se o Seq está acessível:
   ```bash
   curl http://localhost:5341
   ```

4. Verificar logs do container do Seq:
   ```bash
   docker-compose logs seq
   ```

---

## Dicas de Performance

### Reduzir tempo de build

```bash
# Usar build incremental
dotnet build --no-restore

# Ou build em paralelo
dotnet build -m
```

### Reduzir uso de memória do Seq

No docker-compose.yml, adicionar limites:
```yaml
services:
  seq:
    # ... outras configurações
    deploy:
      resources:
        limits:
          memory: 512M
```

---

## Recursos Adicionais

- [Documentação Serilog](https://serilog.net/)
- [Documentação Seq](https://docs.datalust.co/docs)
- [NuGet Troubleshooting](https://learn.microsoft.com/en-us/nuget/consume-packages/troubleshooting)
- [Docker Troubleshooting](https://docs.docker.com/config/daemon/troubleshoot/)
