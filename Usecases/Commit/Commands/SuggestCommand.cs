using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Common;
using Usecases.Git;

namespace Usecases.Commit.Commands;

public sealed class SuggestCommand : AsyncCommand<SuggestCommand.Settings>
{
    private readonly CommitWorkflowService _workflow;
    private readonly IGitService _gitService;

    public SuggestCommand(CommitWorkflowService workflow, IGitService gitService)
    {
        _workflow = workflow;
        _gitService = gitService;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandOption("--commit")]
        [Description("Also create the git commit with the suggested message.")]
        public bool Commit { get; set; }

        [CommandOption("--files <FILES>")]
        [Description("Limit analysis to these staged file paths.")]
        public string[]? Files { get; set; }

        [CommandOption("--full-context")]
        [Description("Include saved project analysis when generating the message.")]
        [DefaultValue(true)]
        public bool FullContext { get; set; } = true;

        [CommandOption("--no-prompt")]
        [Description("Do not prompt to run project analysis when missing.")]
        public bool NoPrompt { get; set; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var repoRoot = _gitService.TryGetRepositoryRoot();
        if (repoRoot is null)
        {
            AnsiConsole.MarkupLine("[red]Not inside a git repository.[/]");
            return 1;
        }

        if (!await _gitService.HasStagedChangesAsync(cancellationToken))
        {
            AnsiConsole.MarkupLine("[yellow]No staged changes. Stage files with git add first.[/]");
            return 1;
        }

        var diff = settings.Files is { Length: > 0 }
            ? await _gitService.GetDiffForFilesAsync(settings.Files, cancellationToken)
            : await _gitService.GetStagedDiffAsync(cancellationToken);

        var (config, model) = await _workflow.GetActiveSetupAsync(cancellationToken);

        string? analysisContent = null;
        if (settings.FullContext)
        {
            var analysis = await _workflow.GetProjectAnalysisAsync(repoRoot, cancellationToken);
            if (analysis is null && !settings.NoPrompt)
            {
                var shouldAnalyze = AnsiConsole.Confirm(
                    "No saved project analysis found. Run analyze-project now?",
                    defaultValue: false);
                if (shouldAnalyze)
                    analysis = await _workflow.AnalyzeProjectAsync(repoRoot, model, cancellationToken: cancellationToken);
            }

            analysisContent = analysis?.Content;
        }

        var message = await _workflow.GenerateCommitMessageAsync(
            diff,
            analysisContent,
            config,
            model,
            cancellationToken);

        AnsiConsole.Write(new Panel(message) { Header = new PanelHeader("Suggested commit message") });

        if (settings.Commit)
        {
            await _gitService.CommitAsync(message, cancellationToken);
            AnsiConsole.MarkupLine("[green]Commit created.[/]");
        }

        return 0;
    }
}
