using Domain.LlmAggregate;
using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Common;

namespace Usecases.Llm.Commands;

public sealed class ListProvidersCommand : Command<ListProvidersCommand.Settings>
{
    public sealed class Settings : BaseSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var table = new Table();
        table.AddColumn("Provider");
        table.AddColumn("Display name");
        table.AddColumn("Example model");
        table.AddColumn("Default endpoint");

        foreach (var provider in LlmProviderCatalog.All)
        {
            table.AddRow(
                provider.Type.ToString(),
                provider.DisplayName,
                provider.ExampleModelId,
                provider.DefaultEndpoint);
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine(
            "[grey]Providers are defined in Domain (enum + catalog), not seeded in SQLite. " +
            "Configure a concrete model with[/] [blue]dot-ai llm add[/][grey].[/]");
        return 0;
    }
}
