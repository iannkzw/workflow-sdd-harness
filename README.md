# Workflow SDD Harness

Orquestrador de coding agents para o fluxo Spec-Driven Development (SDD).

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build

```bash
dotnet build WorkflowSddHarness.sln
```

## Uso

### `config show`

Carrega e valida o `config.json` de um projeto-alvo, exibindo a configuração e os caminhos resolvidos.

```bash
dotnet run --project src/WorkflowSddHarness.Cli -- config show --config <caminho/para/.harness/config.json>
```

**Exemplo:**

```bash
dotnet run --project src/WorkflowSddHarness.Cli -- config show \
  --config tests/WorkflowSddHarness.Infrastructure.Tests/Fixtures/target-project/.harness/config.json
```

**Saída esperada (sucesso, exit 0):**

```
=== Configuração ===
version:   1
providers: claude
steps:     design, execute

=== Caminhos Resolvidos ===
projectRoot: /caminho/para/seu-projeto
specsRoot:   /caminho/para/seu-projeto/.specs/features
rulesDir:    /caminho/para/seu-projeto/.harness/rules
src:         /caminho/para/seu-projeto/src
```

**Cenários de erro (exit 1):**

| Situação | Mensagem |
|----------|----------|
| Arquivo não encontrado | `Erro: Arquivo de configuração não encontrado: <caminho>` |
| Caminho é diretório | `Erro: O caminho aponta para um diretório, não um arquivo: <caminho>` |
| JSON inválido/vazio | `Erro: JSON inválido: <detalhe>` |
| Schema inválido (campo faltando/tipo errado) | `Erro: Campo '<campo>': <mensagem>` |
| `--config` ausente | Mensagem de uso + exit 1 |
| Verbo desconhecido | Lista de comandos disponíveis + exit 1 |

## Testes

### Suíte padrão (sem `claude` instalado)

```bash
dotnet test WorkflowSddHarness.sln --filter "Category!=RealCli"
```

Todos os testes usam o `WorkflowSddHarness.StubCli` como substituto determinístico do `claude`. Roda offline, sem login e sem chave de API.

### Smoke opt-in (contra o `claude` real)

Requer: `claude` no PATH e autenticado.

```bash
dotnet test WorkflowSddHarness.sln --filter "Category=RealCli"
```

O teste auto-pula (`Skipped`) quando `claude` não está no PATH, então a categoria pode ser omitida no filtro padrão sem efeito negativo.

---

## Checklist de E2E manual — fechamento do M1

Execute o smoke opt-in uma vez com o `claude` real logado e cole o JSON observado abaixo como evidência de contrato.

- [ ] `dotnet test --filter "Category=RealCli"` com `claude` no PATH → `1 passed`
- [ ] `IsSuccess = true` no resultado
- [ ] `Text` não vazio e com acentos íntegros (ex.: `ç`, `ã`, `é`, `—`)
- [ ] Campos `result` e `usage` presentes na resposta JSON crua

**JSON observado (colar aqui após execução manual):**

```json
<!-- cole aqui o JSON bruto retornado pelo claude real -->
```

---

## Estrutura do projeto

```
src/
  WorkflowSddHarness.Domain/          # Modelo de domínio (HarnessConfig, ICodingAgentPort, etc.)
  WorkflowSddHarness.Application/     # Casos de uso (vazio — features futuras)
  WorkflowSddHarness.Infrastructure/  # Adapters: ConfigLoader, validators, CodingAgentRunner, ClaudeCliAdapter
  WorkflowSddHarness.Cli/             # Ponto de entrada: CLI com System.CommandLine
tests/
  WorkflowSddHarness.Domain.Tests/
  WorkflowSddHarness.Application.Tests/
  WorkflowSddHarness.Infrastructure.Tests/
  WorkflowSddHarness.Cli.Tests/
  WorkflowSddHarness.StubCli/         # Fixture determinística (substitui o claude nos testes)
```

## Formato do config.json

O harness espera um `config.json` dentro de um diretório `.harness/` na raiz do projeto-alvo:

```jsonc
{
  "version": 1,
  "paths": {
    "specsRoot": ".specs/features",
    "rules": ".harness/rules",
    "state": ".specs/project/STATE.md",
    "observability": ".harness/observability",
    "src": "src"
  },
  "providers": {
    "claude": {
      "enabled": true,
      "cmd": "claude",
      "headlessFlag": "-p",
      "modelFlag": "--model",
      "installHint": "npm install -g @anthropic-ai/claude-code"
    }
  },
  "effortMap": {
    "claude": { "high": "claude-opus-4-8", "medium": "claude-sonnet-4-6", "low": "claude-haiku-4-5" }
  },
  "steps": {
    "design":  { "provider": "claude", "effort": "high",   "maxTokens": 12000 },
    "execute": { "provider": "claude", "effort": "medium", "maxTokens": 6000 }
  },
  "rules": { "apply": ["architecture", "testing"] },
  "gates": {
    "coverage":  { "enabled": true, "policy": "WARN" },
    "lintTasks": { "enabled": true, "policy": "HALT" }
  }
}
```
