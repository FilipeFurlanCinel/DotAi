# DotAi

Local-first CLI that writes AI-assisted git commit messages for the repository you are in.

It reads staged changes, optionally uses a saved project analysis, calls your configured LLM, and creates the commit. Config and SQLite live **per repository** under `.dotai/`.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- `git` on your PATH

## Install (primary — local tool)

From this repository:

```bash
chmod +x scripts/install-local.sh
./scripts/install-local.sh
```

Or manually:

```bash
dotnet pack Presentation/Presentation.csproj -c Release -o ./artifacts/tool
dotnet tool install -g DotAi --add-source ./artifacts/tool --version 0.1.0
```

Ensure tools (and, if needed, `DOTNET_ROOT`) are available:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
# only if the SDK lives under ~/.dotnet (user install):
export DOTNET_ROOT="$HOME/.dotnet"
```

Update after code changes:

```bash
./scripts/install-local.sh
# or: dotnet tool update -g DotAi --add-source ./artifacts/tool
```

Uninstall:

```bash
dotnet tool uninstall -g DotAi
```

### Dev run without installing

```bash
dotnet run --project Presentation -- --help
dotnet run --project Presentation -- llm list
```

## Quick start

In any git repository (including this one):

```bash
dot-ai init
dot-ai llm add gpt-4o-mini -p OpenAi -k "$OPENAI_API_KEY"
git add .
dot-ai                 # default: analyze staged changes → commit
# same as: dot-ai commit
```

### Default commit flow

1. Read currently staged changes
2. Look up saved project analysis in `.dotai/DotAi.db`
   - If missing, prompt to run `analyze-project`
3. Generate a commit message (LLM + optional ticket prefix)
4. Create the git commit

## `.dotai/` layout

Created at the git root of the target repository:

| Path | Purpose |
|------|---------|
| `.dotai/DotAi.db` | SQLite (LLM models, config, project analysis) |
| `.dotai/analysis.md` | Human-readable analysis from `analyze-project` |

`DotAi.db*` is gitignored. You may commit `analysis.md` if you want shared context.

## Commands

| Command | Description |
|---------|-------------|
| `dot-ai` / `dot-ai commit` | Default flow: staged diff → message → commit |
| `dot-ai commit --dry-run` | Generate message without committing |
| `dot-ai suggest` | Print a suggestion (`--commit` to apply) |
| `dot-ai analyze-project` | Analyze repo → DB + `.dotai/analysis.md` |
| `dot-ai init` | Create `.dotai/` and optional LLM wizard |
| `dot-ai llm add <name> -p <Provider> [-k key] [-e endpoint]` | Register a model (`--use` by default) |
| `dot-ai llm list` | List configured models |
| `dot-ai llm providers` | List supported providers (Domain catalog) |
| `dot-ai llm use <name-or-id>` | Set active model |
| `dot-ai llm remove <name-or-id>` | Remove a model |
| `dot-ai config show` | Show active config |
| `dot-ai config set-prefix "TICKET-31"` | Prefix every commit message |
| `dot-ai config clear-prefix` | Clear prefix |
| `dot-ai config set-complexity Conventional` | `Simple`, `Conventional`, or `Detailed` |

### LLM examples

```bash
# List built-in providers (OpenAi / Gemini)
dot-ai llm providers

# OpenAI (GPT)
dot-ai llm add gpt-4o-mini -p OpenAi -k "$OPENAI_API_KEY"
# alias: -p GPT

# Google Gemini
dot-ai llm add gemini-2.0-flash -p Gemini -k "$GEMINI_API_KEY"
# alias: -p Google

dot-ai config set-prefix "TICKET-31"
dot-ai analyze-project
dot-ai suggest
dot-ai commit --dry-run
```

Supported providers (v1): **`OpenAi`** (GPT) and **`Gemini`**.  
They are defined in Domain as `LlmProviderType` + `LlmProviderCatalog` — not seeded into SQLite. SQLite only stores the user-configured `LlmModel` rows (name, key, endpoint). Adding a new vendor later means: extend the enum, catalog entry, and an `ILlmProvider` in Infrastructure.

## Docker (secondary)

Use Docker when you do not want a local SDK, or for one-off demos. For daily commits, prefer the local tool — Docker mounts create ownership friction on `.dotai/` and add ceremony around git.

```bash
docker compose build

# Run against the current directory (or set HOST_REPO)
docker compose run --rm -it dot-ai init
docker compose run --rm -it dot-ai llm list
HOST_REPO=/path/to/your/repo docker compose run --rm -it dot-ai commit
```

Notes:

- Workspace is mounted at `/workspace`; DB at `/workspace/.dotai/DotAi.db`
- Files created in the container may be owned by root. Prefer the local tool for daily use. If you already have a root-owned `.dotai/`:

```bash
sudo chown -R "$USER" .dotai
# or: sudo rm -rf .dotai
```

## Architecture

Clean / DDD layout:

- **Domain** — `LlmModel`, `Configuration`, `ProjectAnalysis`; `LlmProviderType` + `LlmProviderCatalog`
- **Usecases** — Spectre commands + ports (`IGitService`, `ILlmClient`, `ILlmProvider`, …)
- **Infrastructure** — EF Core SQLite, git CLI, per-provider HTTP clients (`OpenAiLlmProvider`, `GeminiLlmProvider`)
- **Presentation** — `dot-ai` host / DI (packaged as a .NET tool)

### Specification pattern

Query logic lives in specifications used with the generic repository:

```csharp
var spec = new LlmModelByNameSpecification("gpt-4o-mini");
var models = await _repository.ListAsync(spec);
```

### Entity modelling

![Entity modelling](Files/img.png)
