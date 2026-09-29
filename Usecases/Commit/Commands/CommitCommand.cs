using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Commit;
using Usecases.Common;
using Usecases.Git;

namespace Usecases.Commit.Commands;

public sealed class CommitCommand : AsyncCommand<CommitCommand.Settings>
{
    private readonly CommitWorkflowService _workflow;
    private readonly IGitService _gitService;

    public CommitCommand(CommitWorkflowService workflow, IGitService gitService)
    {
        _workflow = workflow;
        _gitService = gitService;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandOption("--no-prompt")]
        [Description("Do not prompt to run project analysis when missing.")]
        public bool NoPrompt { get; set; }

        [CommandOption("--files <FILES>")]
        [Description("Limit analysis to these staged file paths.")]
        public string[]? Files { get; set; }

        [CommandOption("--dry-run")]
        [Description("Generate the message but do not create the commit.")]
        public bool DryRun { get; set; }
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

        if (string.IsNullOrWhiteSpace(diff))
        {
            AnsiConsole.MarkupLine("[yellow]Staged diff is empty.[/]");
            return 1;
        }

        var (config, model) = await _workflow.GetActiveSetupAsync(cancellationToken);
        var analysis = await _workflow.GetProjectAnalysisAsync(repoRoot, cancellationToken);

        if (analysis is null && !settings.NoPrompt)
        {
            var shouldAnalyze = AnsiConsole.Confirm(
                "No saved project analysis found. Run analyze-project to improve commit quality?",
                defaultValue: true);

            if (shouldAnalyze)
            {
                analysis = await _workflow.AnalyzeProjectAsync(repoRoot, model, cancellationToken: cancellationToken);
                AnsiConsole.MarkupLine($"[green]Analysis saved to[/] {DotAiPaths.GetAnalysisPath(repoRoot)}");
            }
        }

        AnsiConsole.MarkupLine("[grey]Generating commit message...[/]");
        var message = await _workflow.GenerateCommitMessageAsync(
            diff,
            analysis?.Content,
            config,
            model,
            cancellationToken);

        AnsiConsole.Write(new Panel(message) { Header = new PanelHeader("Commit message") });

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run — commit not created.[/]");
            return 0;
        }

        await _gitService.CommitAsync(message, cancellationToken);
        AnsiConsole.MarkupLine("[green]Commit created.[/]");
        return 0;
    }
}
