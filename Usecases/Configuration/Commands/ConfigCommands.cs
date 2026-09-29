using System.ComponentModel;
using Domain.ConfigurationAggregate;
using Domain.ConfigurationAggregate.Enums;
using Domain.ConfigurationAggregate.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Common;
using Usecases.Configuration.Specifications;
using ConfigurationModel = Domain.ConfigurationAggregate.Models.Configuration;

namespace Usecases.Configuration.Commands;

public sealed class ShowConfigCommand : AsyncCommand<ShowConfigCommand.Settings>
{
    private readonly IRepository<ConfigurationModel> _configurationRepository;

    public ShowConfigCommand(IRepository<ConfigurationModel> configurationRepository)
    {
        _configurationRepository = configurationRepository;
    }

    public sealed class Settings : BaseSettings
    {
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var config = await _configurationRepository.GetBySpecAsync(new EnabledConfigurationSpecification());
        if (config is null)
        {
            AnsiConsole.MarkupLine("[yellow]No enabled configuration. Run 'dot-ai init'.[/]");
            return 1;
        }

        var table = new Table().HideHeaders();
        table.AddColumn("Key");
        table.AddColumn("Value");
        table.AddRow("Name", config.Name);
        table.AddRow("Enabled", config.IsEnabled.ToString());
        table.AddRow("LLM Id", config.LlmModelId.ToString());
        table.AddRow("LLM Name", config.LlmModel?.Name ?? "-");
        table.AddRow("Provider", config.LlmModel?.ProviderType.ToString() ?? "-");
        table.AddRow("Prefix", config.CommitPrefix ?? "(none)");
        table.AddRow("Complexity", config.PromptConfig?.ComplexityLevel.ToString() ?? "-");
        table.AddRow("MaxTokens", config.PromptConfig?.MaxTokens.ToString() ?? "-");
        AnsiConsole.Write(table);
        return 0;
    }
}

public sealed class SetPrefixCommand : AsyncCommand<SetPrefixCommand.Settings>
{
    private readonly IRepository<ConfigurationModel> _configurationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPrefixCommand(
        IRepository<ConfigurationModel> configurationRepository,
        IUnitOfWork unitOfWork)
    {
        _configurationRepository = configurationRepository;
        _unitOfWork = unitOfWork;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<PREFIX>")]
        [Description("Ticket prefix, e.g. TICKET-31.")]
        public string Prefix { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var config = await RequireConfigAsync();
        if (config is null)
            return 1;

        var updated = config.WithPrefix(settings.Prefix);
        await _configurationRepository.UpdateAsync(updated);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        AnsiConsole.MarkupLine($"[green]Commit prefix set to[/] {updated.CommitPrefix}");
        return 0;
    }

    private async Task<ConfigurationModel?> RequireConfigAsync()
    {
        var config = await _configurationRepository.GetBySpecAsync(new EnabledConfigurationSpecification());
        if (config is null)
            AnsiConsole.MarkupLine("[red]No enabled configuration. Run 'dot-ai init'.[/]");
        return config;
    }
}

public sealed class ClearPrefixCommand : AsyncCommand<ClearPrefixCommand.Settings>
{
    private readonly IRepository<ConfigurationModel> _configurationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClearPrefixCommand(
        IRepository<ConfigurationModel> configurationRepository,
        IUnitOfWork unitOfWork)
    {
        _configurationRepository = configurationRepository;
        _unitOfWork = unitOfWork;
    }

    public sealed class Settings : BaseSettings
    {
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var config = await _configurationRepository.GetBySpecAsync(new EnabledConfigurationSpecification());
        if (config is null)
        {
            AnsiConsole.MarkupLine("[red]No enabled configuration. Run 'dot-ai init'.[/]");
            return 1;
        }

        var updated = config.ClearPrefix();
        await _configurationRepository.UpdateAsync(updated);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        AnsiConsole.MarkupLine("[green]Commit prefix cleared.[/]");
        return 0;
    }
}

public sealed class SetComplexityCommand : AsyncCommand<SetComplexityCommand.Settings>
{
    private readonly IRepository<ConfigurationModel> _configurationRepository;
    private readonly IRepository<PromptTemplate> _promptTemplateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetComplexityCommand(
        IRepository<ConfigurationModel> configurationRepository,
        IRepository<PromptTemplate> promptTemplateRepository,
        IUnitOfWork unitOfWork)
    {
        _configurationRepository = configurationRepository;
        _promptTemplateRepository = promptTemplateRepository;
        _unitOfWork = unitOfWork;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<LEVEL>")]
        [Description("Simple, Conventional, or Detailed.")]
        public string Level { get; set; } = "Conventional";
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CommitComplexityLevel>(settings.Level, ignoreCase: true, out var level))
        {
            AnsiConsole.MarkupLine("[red]Invalid level.[/] Use Simple, Conventional, or Detailed.");
            return 1;
        }

        var config = await _configurationRepository.GetBySpecAsync(new EnabledConfigurationSpecification());
        if (config is null)
        {
            AnsiConsole.MarkupLine("[red]No enabled configuration. Run 'dot-ai init'.[/]");
            return 1;
        }

        var template = DefaultPromptTemplates.CreateFor(level);
        await _promptTemplateRepository.AddAsync(template);

        config.PromptConfig.Update(level, config.PromptConfig.MaxTokens, template.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        AnsiConsole.MarkupLine($"[green]Complexity set to[/] {level}");
        return 0;
    }
}
