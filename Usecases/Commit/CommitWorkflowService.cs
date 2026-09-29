using Domain.ConfigurationAggregate;
using Domain.ConfigurationAggregate.Enums;
using Domain.ConfigurationAggregate.Models;
using Domain.LlmAggregate.Models;
using Spectre.Console;
using Usecases.Common;
using Usecases.Configuration.Specifications;
using Usecases.Git;
using Usecases.Llm;
using Usecases.ProjectAnalysis;
using Usecases.ProjectAnalysis.Specifications;
using ConfigurationModel = Domain.ConfigurationAggregate.Models.Configuration;
using ProjectAnalysisModel = Domain.ProjectAnalysisAggregate.Models.ProjectAnalysis;

namespace Usecases.Commit;

public sealed class CommitWorkflowService
{
    private readonly IRepository<ConfigurationModel> _configurationRepository;
    private readonly IRepository<LlmModel> _llmRepository;
    private readonly IRepository<ProjectAnalysisModel> _analysisRepository;
    private readonly IRepository<PromptTemplate> _promptTemplateRepository;
    private readonly IRepository<PromptConfiguration> _promptConfigurationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGitService _gitService;
    private readonly ILlmClient _llmClient;
    private readonly IProjectAnalysisWriter _analysisWriter;

    public CommitWorkflowService(
        IRepository<ConfigurationModel> configurationRepository,
        IRepository<LlmModel> llmRepository,
        IRepository<ProjectAnalysisModel> analysisRepository,
        IRepository<PromptTemplate> promptTemplateRepository,
        IRepository<PromptConfiguration> promptConfigurationRepository,
        IUnitOfWork unitOfWork,
        IGitService gitService,
        ILlmClient llmClient,
        IProjectAnalysisWriter analysisWriter)
    {
        _configurationRepository = configurationRepository;
        _llmRepository = llmRepository;
        _analysisRepository = analysisRepository;
        _promptTemplateRepository = promptTemplateRepository;
        _promptConfigurationRepository = promptConfigurationRepository;
        _unitOfWork = unitOfWork;
        _gitService = gitService;
        _llmClient = llmClient;
        _analysisWriter = analysisWriter;
    }

    public async Task<(ConfigurationModel Config, LlmModel Model)> GetActiveSetupAsync(
        CancellationToken cancellationToken = default)
    {
        var config = await _configurationRepository.GetBySpecAsync(new EnabledConfigurationSpecification());
        if (config is null)
            throw new InvalidOperationException(
                "No enabled configuration found. Run 'dot-ai init' or 'dot-ai llm add' first.");

        var model = config.LlmModel ?? await _llmRepository.GetByIdAsync(config.LlmModelId);
        if (model is null)
            throw new InvalidOperationException("Configured LLM model was not found. Run 'dot-ai llm use'.");

        return (config, model);
    }

    public async Task<string> GenerateCommitMessageAsync(
        string diff,
        string? projectAnalysis,
        ConfigurationModel config,
        LlmModel model,
        CancellationToken cancellationToken = default)
    {
        var complexity = config.PromptConfig?.ComplexityLevel ?? CommitComplexityLevel.Conventional;
        var maxTokens = config.PromptConfig?.MaxTokens > 0 ? config.PromptConfig.MaxTokens : 1000;
        var systemPrompt = config.PromptConfig?.PromptTemplate?.Content;
        if (string.IsNullOrWhiteSpace(systemPrompt))
            systemPrompt = DefaultPromptTemplates.BuildSystemPrompt(complexity, config.CommitPrefix);
        else if (!string.IsNullOrWhiteSpace(config.CommitPrefix))
            systemPrompt += $"\nAlways start with the ticket prefix \"{config.CommitPrefix}\".";

        var userPrompt = BuildCommitUserPrompt(diff, projectAnalysis);
        var message = await _llmClient.CompleteAsync(model, systemPrompt, userPrompt, maxTokens, cancellationToken);
        return CleanCommitMessage(message, config.CommitPrefix);
    }

    public async Task<ProjectAnalysisModel?> GetProjectAnalysisAsync(
        string repositoryRoot,
        CancellationToken cancellationToken = default)
    {
        return await _analysisRepository.GetBySpecAsync(
            new ProjectAnalysisBySourceRootSpecification(repositoryRoot));
    }

    public async Task<ProjectAnalysisModel> AnalyzeProjectAsync(
        string repositoryRoot,
        LlmModel model,
        int maxTokens = 2000,
        CancellationToken cancellationToken = default,
        IAnsiConsole? console = null)
    {
        console ??= AnsiConsole.Console;
        var files = await _gitService.ListTrackedFilesAsync(cancellationToken);
        var context = await BuildProjectContextAsync(repositoryRoot, files, cancellationToken);

        var systemPrompt = """
            You are analyzing a software repository to help generate better git commit messages later.
            Produce a concise markdown summary covering: purpose, main technologies, architecture/layout,
            important modules, and coding conventions you can infer.
            Do not invent details that are not supported by the provided excerpts.
            """;

        var userPrompt = $"""
            Repository root: {repositoryRoot}
            Tracked file count: {files.Count}

            File excerpts:
            {context}
            """;

        console.MarkupLine("[grey]Calling LLM for project analysis...[/]");
        var content = await _llmClient.CompleteAsync(model, systemPrompt, userPrompt, maxTokens, cancellationToken);
        var summary = content.Length > 280 ? content[..280] + "..." : content;

        var existing = await GetProjectAnalysisAsync(repositoryRoot, cancellationToken);
        ProjectAnalysisModel analysis;
        if (existing is null)
        {
            analysis = ProjectAnalysisModel.Create(repositoryRoot, content, summary);
            await _analysisRepository.AddAsync(analysis);
        }
        else
        {
            analysis = existing.Refresh(content, summary);
            await _analysisRepository.UpdateAsync(analysis);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _analysisWriter.WriteAsync(repositoryRoot, content, cancellationToken);
        return analysis;
    }

    public async Task EnsureDefaultConfigurationAsync(
        LlmModel model,
        CommitComplexityLevel complexity = CommitComplexityLevel.Conventional,
        CancellationToken cancellationToken = default)
    {
        var enabled = await _configurationRepository.GetBySpecAsync(new EnabledConfigurationSpecification());
        if (enabled is not null)
        {
            var updated = enabled.WithLlm(model.Id).Enable();
            await _configurationRepository.UpdateAsync(updated);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var template = DefaultPromptTemplates.CreateFor(complexity);
        await _promptTemplateRepository.AddAsync(template);

        var promptConfig = PromptConfiguration.Create(complexity, template.Id);
        await _promptConfigurationRepository.AddAsync(promptConfig);

        var configuration = ConfigurationModel.Create("default", model.Id, promptConfig, isEnabled: true);
        await _configurationRepository.AddAsync(configuration);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string BuildCommitUserPrompt(string diff, string? projectAnalysis)
    {
        var analysisBlock = string.IsNullOrWhiteSpace(projectAnalysis)
            ? "(No saved project analysis.)"
            : projectAnalysis;

        return $"""
            Project analysis:
            {analysisBlock}

            Staged diff:
            ```
            {diff}
            ```

            Write the commit message for these staged changes.
            """;
    }

    private static string CleanCommitMessage(string message, string? prefix)
    {
        var cleaned = message.Trim().Trim('"');
        if (cleaned.StartsWith("```"))
        {
            var lines = cleaned.Split('\n').ToList();
            if (lines.Count > 0 && lines[0].StartsWith("```"))
                lines.RemoveAt(0);
            if (lines.Count > 0 && lines[^1].Trim().StartsWith("```"))
                lines.RemoveAt(lines.Count - 1);
            cleaned = string.Join('\n', lines).Trim();
        }

        if (!string.IsNullOrWhiteSpace(prefix) &&
            !cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            cleaned = $"{prefix} {cleaned}";
        }

        return cleaned;
    }

    private static async Task<string> BuildProjectContextAsync(
        string repositoryRoot,
        IReadOnlyList<string> files,
        CancellationToken cancellationToken)
    {
        var preferred = files
            .Where(IsUsefulPath)
            .OrderBy(Priority)
            .ThenBy(f => f.Length)
            .Take(40)
            .ToList();

        var chunks = new List<string>();
        var totalChars = 0;
        const int maxChars = 40_000;

        foreach (var relative in preferred)
        {
            var fullPath = Path.Combine(repositoryRoot, relative);
            if (!File.Exists(fullPath))
                continue;

            string text;
            try
            {
                text = await File.ReadAllTextAsync(fullPath, cancellationToken);
            }
            catch
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(text))
                continue;

            if (text.Length > 2500)
                text = text[..2500] + "\n... (truncated)";

            var block = $"### {relative}\n{text}\n";
            if (totalChars + block.Length > maxChars)
                break;

            chunks.Add(block);
            totalChars += block.Length;
        }

        return chunks.Count == 0
            ? "(No readable source files found.)"
            : string.Join('\n', chunks);
    }

    private static bool IsUsefulPath(string path)
    {
        var normalized = path.Replace('\\', '/');
        if (normalized.StartsWith(".dotai/", StringComparison.OrdinalIgnoreCase))
            return false;
        if (normalized.Contains("/bin/") || normalized.Contains("/obj/"))
            return false;

        if (normalized.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("docker-compose.yml", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("Readme.md", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("README.md", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var ext = Path.GetExtension(normalized).ToLowerInvariant();
        return ext is ".cs" or ".csproj" or ".sln" or ".md" or ".json" or ".yml" or ".yaml"
            or ".ts" or ".tsx" or ".js" or ".jsx" or ".py" or ".go" or ".rs" or ".java";
    }

    private static int Priority(string path)
    {
        var name = Path.GetFileName(path);
        if (name.Equals("README.md", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Readme.md", StringComparison.OrdinalIgnoreCase))
            return 0;
        if (path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            return 1;
        if (path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            return 2;
        if (path.Contains("Program.cs", StringComparison.OrdinalIgnoreCase))
            return 3;
        return 10;
    }
}
