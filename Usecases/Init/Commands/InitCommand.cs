using System.ComponentModel;
using Domain.LlmAggregate;
using Domain.LlmAggregate.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Commit;
using Usecases.Common;
using Usecases.Git;

namespace Usecases.Init.Commands;

public sealed class InitCommand : AsyncCommand<InitCommand.Settings>
{
    private readonly IGitService _gitService;
    private readonly IRepository<LlmModel> _llmRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CommitWorkflowService _workflow;

    public InitCommand(
        IGitService gitService,
        IRepository<LlmModel> llmRepository,
        IUnitOfWork unitOfWork,
        CommitWorkflowService workflow)
    {
        _gitService = gitService;
        _llmRepository = llmRepository;
        _unitOfWork = unitOfWork;
        _workflow = workflow;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandOption("--skip-wizard")]
        [Description("Only create .dotai and database, skip LLM setup.")]
        public bool SkipWizard { get; set; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var repoRoot = _gitService.TryGetRepositoryRoot()
                       ?? Environment.GetEnvironmentVariable("DOTAI_WORKSPACE")
                       ?? Directory.GetCurrentDirectory();

        DotAiPaths.EnsureDotAiDirectory(repoRoot);
        AnsiConsole.MarkupLine($"[green]Initialized[/] {DotAiPaths.GetDotAiDirectory(repoRoot)}");

        if (settings.SkipWizard)
            return 0;

        var existing = await _llmRepository.GetAllAsync();
        if (existing.Count > 0)
        {
            AnsiConsole.MarkupLine("[grey]LLM models already configured. Use 'dot-ai llm list'.[/]");
            return 0;
        }

        if (!AnsiConsole.Confirm("Configure an LLM now?", defaultValue: true))
            return 0;

        var choices = LlmProviderCatalog.All
            .Select(p => $"{p.Type} — {p.DisplayName} (e.g. {p.ExampleModelId})")
            .ToList();

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select provider")
                .AddChoices(choices));

        var provider = LlmProviderCatalog.All
            .First(p => selected.StartsWith(p.Type.ToString(), StringComparison.Ordinal))
            .Type;
        var info = LlmProviderCatalog.Get(provider);

        var name = AnsiConsole.Ask("Model name (provider model id):", info.ExampleModelId);
        var apiKey = AnsiConsole.Prompt(new TextPrompt<string>("API key:").Secret());
        var endpoint = AnsiConsole.Prompt(
            new TextPrompt<string>($"Endpoint (optional, default {info.DefaultEndpoint}):").AllowEmpty());

        var model = LlmModel.Create(
            name,
            provider,
            string.IsNullOrWhiteSpace(apiKey) ? null : apiKey,
            string.IsNullOrWhiteSpace(endpoint) ? null : endpoint);

        await _llmRepository.AddAsync(model);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _workflow.EnsureDefaultConfigurationAsync(model, cancellationToken: cancellationToken);

        AnsiConsole.MarkupLine($"[green]Ready.[/] Active model: {model.Name} ({info.DisplayName})");
        return 0;
    }
}
