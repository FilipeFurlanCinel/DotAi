using Spectre.Console.Cli;
using Usecases.Configuration.Commands;

namespace Presentation.Configuration;

public static class ConfigurationCommands
{
    public static void ConfigureConfigurationCommands(this IConfigurator config)
    {
        config.AddBranch("config", branch =>
        {
            branch.SetDescription("Manage commit configuration");

            branch.AddCommand<ShowConfigCommand>("show")
                .WithDescription("Show the active configuration.");

            branch.AddCommand<SetPrefixCommand>("set-prefix")
                .WithDescription("Set a ticket prefix for commit messages.");

            branch.AddCommand<ClearPrefixCommand>("clear-prefix")
                .WithDescription("Clear the commit message prefix.");

            branch.AddCommand<SetComplexityCommand>("set-complexity")
                .WithDescription("Set commit message complexity (Simple, Conventional, Detailed).");
        });
    }
}
