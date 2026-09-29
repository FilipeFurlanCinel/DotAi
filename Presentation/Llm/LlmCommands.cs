using Spectre.Console.Cli;
using Usecases.Llm.Commands;

namespace Presentation.Llm;

public static class LlmCommands
{
    public static void ConfigureLlmCommands(this IConfigurator config)
    {
        config.AddBranch("llm", llm =>
        {
            llm.SetDescription("Manage LLM models");

            llm.AddCommand<AddLlmModelCommand>("add")
                .WithDescription("Add an LLM model (provider, API key, endpoint).");

            llm.AddCommand<ListLlmModelsCommand>("list")
                .WithDescription("List configured LLM models.");

            llm.AddCommand<ListProvidersCommand>("providers")
                .WithDescription("List supported LLM providers (Domain catalog).");

            llm.AddCommand<RemoveLlmModelCommand>("remove")
                .WithDescription("Remove an LLM model by name or id.");

            llm.AddCommand<UseLlmModelCommand>("use")
                .WithDescription("Set the active LLM model.");
        });
    }
}
