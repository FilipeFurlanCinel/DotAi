using Domain.ConfigurationAggregate.Enums;
using Domain.ConfigurationAggregate.Models;

namespace Domain.ConfigurationAggregate;

public static class DefaultPromptTemplates
{
    public static PromptTemplate CreateFor(CommitComplexityLevel level) => level switch
    {
        CommitComplexityLevel.Simple => PromptTemplate.Create(
            "simple-commit",
            """
            You write concise one-line git commit messages.
            Reply with ONLY the commit message text. No quotes, no markdown, no explanation.
            """,
            "Simple one-line commit messages"),

        CommitComplexityLevel.Conventional => PromptTemplate.Create(
            "conventional-commit",
            """
            You write git commit messages that follow Conventional Commits.
            Format: <type>(optional scope): <description>
            Types: feat, fix, docs, style, refactor, perf, test, build, ci, chore.
            Reply with ONLY the commit message text. No quotes, no markdown fences, no explanation.
            """,
            "Conventional Commits style"),

        CommitComplexityLevel.Detailed => PromptTemplate.Create(
            "detailed-commit",
            """
            You write detailed git commit messages with a short subject line and a body.
            Subject: imperative mood, ~50 chars.
            Body: explain why, wrap at ~72 chars.
            Reply with ONLY the commit message text. No markdown fences, no explanation outside the message.
            """,
            "Detailed subject + body"),

        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };

    public static string BuildSystemPrompt(CommitComplexityLevel level, string? commitPrefix)
    {
        var template = CreateFor(level).Content;
        if (string.IsNullOrWhiteSpace(commitPrefix))
            return template;

        return template + $"""

            Always start the commit message with the ticket prefix "{commitPrefix.Trim()}" followed by a space
            (or ":" if using conventional commits after the type/scope). Prefer: "{commitPrefix.Trim()} <message>".
            """;
    }
}
