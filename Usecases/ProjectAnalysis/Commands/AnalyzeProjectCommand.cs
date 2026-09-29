using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Commit;
using Usecases.Common;
using Usecases.Git;

namespace Usecases.ProjectAnalysis.Commands;

public sealed class AnalyzeProjectCommand : AsyncCommand<AnalyzeProjectCommand.Settings>
{
    private readonly CommitWorkflowService _workflow;
    private readonly IGitService _gitService;

    public AnalyzeProjectCommand(CommitWorkflowService workflow, IGitService gitService)
    {
        _workflow = workflow;
        _gitService = gitService;
    }

    public sealed class Settings : BaseSettings
    {
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

        DotAiPaths.EnsureDotAiDirectory(repoRoot);
        var (_, model) = await _workflow.GetActiveSetupAsync(cancellationToken);

        await AnsiConsole.Status()
            .StartAsync("Analyzing project...", async _ =>
            {
                await _workflow.AnalyzeProjectAsync(repoRoot, model, cancellationToken: cancellationToken);
            });

        AnsiConsole.MarkupLine($"[green]Project analysis saved to DB and[/] {DotAiPaths.GetAnalysisPath(repoRoot)}");
        return 0;
    }
}
