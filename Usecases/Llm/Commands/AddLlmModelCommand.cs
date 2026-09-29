using System.ComponentModel;
using Domain.LlmAggregate;
using Domain.LlmAggregate.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Commit;
using Usecases.Common;
using Usecases.Llm.Specifications;

namespace Usecases.Llm.Commands;

public sealed class AddLlmModelCommand : AsyncCommand<AddLlmModelCommand.Settings>
{
    private readonly IRepository<LlmModel> _llmRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CommitWorkflowService _workflow;

    public AddLlmModelCommand(
        IRepository<LlmModel> llmRepository,
        IUnitOfWork unitOfWork,
        CommitWorkflowService workflow)
    {
        _llmRepository = llmRepository;
        _unitOfWork = unitOfWork;
        _workflow = workflow;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<NAME>")]
        [Description("Model id for the provider (e.g. gpt-4o-mini, gemini-2.0-flash).")]
        public string Name { get; set; } = string.Empty;

        [CommandOption("-p|--provider <PROVIDER>")]
        [Description("Provider: OpenAi (GPT) or Gemini.")]
        public string Provider { get; set; } = "OpenAi";

        [CommandOption("-k|--api-key <KEY>")]
        [Description("API key for the provider.")]
        public string? ApiKey { get; set; }

        [CommandOption("-e|--endpoint <URL>")]
        [Description("Optional custom API base URL / endpoint.")]
        public string? Endpoint { get; set; }

        [CommandOption("--use")]
        [Description("Set this model as the active configuration target.")]
        [DefaultValue(true)]
        public bool Use { get; set; } = true;
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        if (!LlmProviderCatalog.TryParse(settings.Provider, out var provider))
        {
            AnsiConsole.MarkupLine(
                $"[red]Invalid provider.[/] Supported: {string.Join(", ", LlmProviderCatalog.Names)} (aliases: GPT, Google).");
            return 1;
        }

        var info = LlmProviderCatalog.Get(provider);
        if (info.RequiresApiKey && string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            AnsiConsole.MarkupLine($"[red]{info.DisplayName} requires an API key (-k/--api-key).[/]");
            return 1;
        }

        var existing = await _llmRepository.GetBySpecAsync(new LlmModelByNameSpecification(settings.Name));
        if (existing is not null)
        {
            AnsiConsole.MarkupLine($"[yellow]Model '{settings.Name}' already exists ({existing.Id}).[/]");
            return 1;
        }

        var model = LlmModel.Create(settings.Name, provider, settings.ApiKey, settings.Endpoint);
        await _llmRepository.AddAsync(model);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (settings.Use)
            await _workflow.EnsureDefaultConfigurationAsync(model, cancellationToken: cancellationToken);

        AnsiConsole.MarkupLine($"[green]Added LLM[/] {model.Name} ([blue]{model.ProviderType}[/]) id={model.Id}");
        return 0;
    }
}
