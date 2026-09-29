using Presentation.Commit;
using Presentation.Common.Interceptors;
using Presentation.Configuration;
using Presentation.Init;
using Presentation.Llm;
using Presentation.ProjectAnalysis;
using Spectre.Console.Cli;

namespace Presentation;

public static class CommandConfiguration
{
    public static void ConfigureCommands(this IConfigurator config)
    {
        config.SetApplicationName("dot-ai");
        config.Settings.ApplicationName = "dot-ai CLI";
        config.SetInterceptor(new GlobalExceptionInterceptor());

        config.ConfigureCommitCommands();
        config.ConfigureLlmCommands();
        config.ConfigureConfigurationCommands();
        config.ConfigureProjectAnalysisCommands();
        config.ConfigureInitCommands();
    }
}
