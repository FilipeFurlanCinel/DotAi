using System.ComponentModel;
using Domain.LlmAggregate.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Commit;
using Usecases.Common;
using Usecases.Llm.Specifications;

namespace Usecases.Llm.Commands;

public sealed class UseLlmModelCommand : AsyncCommand<UseLlmModelCommand.Settings>
{
    private readonly IRepository<LlmModel> _llmRepository;
    private readonly CommitWorkflowService _workflow;

    public UseLlmModelCommand(IRepository<LlmModel> llmRepository, CommitWorkflowService workflow)
    {
        _llmRepository = llmRepository;
        _workflow = workflow;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<NAME_OR_ID>")]
        [Description("Model name or Id to activate.")]
        public string NameOrId { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        LlmModel? model = null;
        if (Guid.TryParse(settings.NameOrId, out var id))
            model = await _llmRepository.GetByIdAsync(id);

        model ??= await _llmRepository.GetBySpecAsync(new LlmModelByNameSpecification(settings.NameOrId));

        if (model is null)
        {
            AnsiConsole.MarkupLine($"[red]Model '{settings.NameOrId}' not found.[/]");
            return 1;
        }

        await _workflow.EnsureDefaultConfigurationAsync(model, cancellationToken: cancellationToken);
        AnsiConsole.MarkupLine($"[green]Active LLM set to[/] {model.Name}");
        return 0;
    }
}
