using Domain._Common;

namespace Domain.ProjectAnalysisAggregate.Models;

public sealed class ProjectAnalysis : AggregateRoot<Guid>
{
    public string SourceRoot { get; }
    public string Content { get; }
    public string? Summary { get; }

    private ProjectAnalysis(
        Guid id,
        string sourceRoot,
        string content,
        string? summary) : base(id)
    {
        SourceRoot = sourceRoot;
        Content = content;
        Summary = summary;
    }

    public static ProjectAnalysis Create(string sourceRoot, string content, string? summary = null)
    {
        if (string.IsNullOrWhiteSpace(sourceRoot))
            throw new ArgumentException("Source root must be provided.", nameof(sourceRoot));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content must be provided.", nameof(content));

        return new ProjectAnalysis(
            id: Guid.NewGuid(),
            sourceRoot: sourceRoot.Trim(),
            content: content.Trim(),
            summary: string.IsNullOrWhiteSpace(summary) ? null : summary.Trim());
    }

    public static ProjectAnalysis Reconstitute(
        Guid id,
        string sourceRoot,
        string content,
        string? summary)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(sourceRoot))
            throw new ArgumentException("Source root must be provided.", nameof(sourceRoot));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content must be provided.", nameof(content));

        return new ProjectAnalysis(id, sourceRoot.Trim(), content.Trim(),
            string.IsNullOrWhiteSpace(summary) ? null : summary.Trim());
    }

    public ProjectAnalysis Refresh(string content, string? summary = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content must be provided.", nameof(content));

        return new ProjectAnalysis(
            id: Id,
            sourceRoot: SourceRoot,
            content: content.Trim(),
            summary: string.IsNullOrWhiteSpace(summary) ? null : summary.Trim());
    }
}
