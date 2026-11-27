# Filas limitadas com Channels no ASP.NET Core

Uma API enfileira tarefas simuladas em memória e sinaliza quando a capacidade se esgota. O laboratório funciona em um processo local sem broker, banco de dados ou credenciais.

[Leia o artigo](article/ARTICLE.md) · [Artigo para LinkedIn](article/ARTICLE_LINKEDIN.md) · [Post para LinkedIn](article/POST_LINKEDIN.md) · [Catálogo .NET](../README.md)

## Pré-requisitos

- [SDK do .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- [curl para executar os comandos HTTP em um terminal](https://curl.se/download.html).

## Começo rápido

Na pasta deste laboratório, inicie a API em loopback.

```bash
dotnet run --project src/BoundedJobs.Api --no-launch-profile -- --urls http://127.0.0.1:19805
```

## Sature a fila e veja a recuperação

Em outro terminal, use Bash para enviar uma tarefa inicial e, logo depois, outras cinco em paralelo. Com uma tarefa em execução e duas aguardando, a pressão produz respostas `503` com `Retry-After: 1`, além de respostas `202` com o `id` aceito. Confira os IDs concluídos no log do servidor. O número de `503` pode variar de acordo com o escalonamento local.

```bash
curl -sS -w ' %{http_code}\n' http://127.0.0.1:19805/jobs \
  -H 'Content-Type: application/json' -d '{"units":5}'
sleep 0.15
for ((request=0; request<5; request++)); do
  (
    response=$(curl -sS -w ' %{http_code}' http://127.0.0.1:19805/jobs \
      -H 'Content-Type: application/json' -d '{"units":5}')
    printf '%s\n' "$response"
  ) &
done
wait
```

No log do servidor, aguarde uma linha `Completed job` para cada `id` aceito.
Envie outra tarefa e confirme a resposta `202`. O comando `wait` do lote
aguarda apenas os processos `curl`.

```bash
curl -si -w '\n' http://127.0.0.1:19805/jobs \
  -H 'Content-Type: application/json' -d '{"units":1}'
```

Encerre a API com `Ctrl+C`.

## Execute os testes

As verificações usam um processador controlado para ocupar a fila sem depender de pausas cronometradas.

```bash
dotnet test ../tests/BoundedJobs.Tests
```

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
