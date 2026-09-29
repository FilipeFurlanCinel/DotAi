using Spectre.Console.Cli;
using Usecases.Init.Commands;

namespace Presentation.Init;

public static class InitCommands
{
    public static void ConfigureInitCommands(this IConfigurator config)
    {
        config.AddCommand<InitCommand>("init")
            .WithDescription("Initialize .dotai storage and optionally configure an LLM.");
    }
}
