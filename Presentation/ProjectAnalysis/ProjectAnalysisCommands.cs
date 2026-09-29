using Spectre.Console.Cli;
using Usecases.ProjectAnalysis.Commands;

namespace Presentation.ProjectAnalysis;

public static class ProjectAnalysisCommands
{
    public static void ConfigureProjectAnalysisCommands(this IConfigurator config)
    {
        config.AddCommand<AnalyzeProjectCommand>("analyze-project")
            .WithDescription("Analyze the repository and save context for better commit messages.");
    }
}
