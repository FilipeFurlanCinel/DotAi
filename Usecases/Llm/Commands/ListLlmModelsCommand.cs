using Domain.LlmAggregate.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Common;
using Usecases.Configuration.Specifications;
using ConfigurationDomain = Domain.ConfigurationAggregate.Models.Configuration;

namespace Usecases.Llm.Commands;

public class ListLlmModelsCommand : AsyncCommand<ListLlmModelsCommand.Settings>
{
    private readonly IRepository<LlmModel> _llmRepository;
    private readonly IRepository<ConfigurationDomain> _configurationRepository;
    private readonly ILogger<ListLlmModelsCommand> _logger;

    public ListLlmModelsCommand(
        IRepository<LlmModel> llmRepository,
        IRepository<ConfigurationDomain> configurationRepository,
        ILogger<ListLlmModelsCommand> logger)
    {
        _llmRepository = llmRepository;
        _configurationRepository = configurationRepository;
        _logger = logger;
    }

    public sealed class Settings : BaseSettings
    {
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var enabledConfiguration = await _configurationRepository.GetBySpecAsync(
            new EnabledConfigurationSpecification());
        var llmModels = await _llmRepository.GetAllAsync();
        var enabledLlmModelId = enabledConfiguration?.LlmModelId;

        if (enabledLlmModelId is null)
            _logger.LogWarning("No active LLM model configuration found.");

        if (llmModels.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No LLM models configured. Run 'dot-ai llm add'.[/]");
            return 0;
        }

        var table = new Table();
        table.AddColumn("Active");
        table.AddColumn("ID");
        table.AddColumn("Name");
        table.AddColumn("Provider");
        table.AddColumn("Endpoint");

        foreach (var model in llmModels)
        {
            var isActive = model.Id == enabledLlmModelId;
            table.AddRow(
                isActive ? "[green]yes[/]" : "",
                model.Id.ToString(),
                isActive ? $"[bold green]{model.Name}[/]" : model.Name,
                model.ProviderType.ToString(),
                model.Endpoint ?? "-");
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
