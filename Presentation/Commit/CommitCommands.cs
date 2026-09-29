using Spectre.Console.Cli;
using Usecases.Commit.Commands;

namespace Presentation.Commit;

public static class CommitCommands
{
    public static void ConfigureCommitCommands(this IConfigurator config)
    {
        config.AddCommand<CommitCommand>("commit")
            .WithDescription("Analyze staged changes and create an AI-generated commit.");

        config.AddCommand<SuggestCommand>("suggest")
            .WithDescription("Suggest a commit message for staged changes (optionally commit).");
    }
}
