using Domain._Common;
using Domain.ConfigurationAggregate.Enums;

namespace Domain.ConfigurationAggregate.Models;

public sealed class PromptConfiguration : Entity<Guid>
{
    public CommitComplexityLevel ComplexityLevel { get; private set; }
    public int MaxTokens { get; private set; }
    public Guid PromptTemplateId { get; private set; }
    public PromptTemplate PromptTemplate { get; private set; } = null!;
 
    private PromptConfiguration(
        Guid id,
        CommitComplexityLevel complexityLevel,
        int maxTokens,
        Guid promptTemplateId) : base(id)
    {
        ComplexityLevel = complexityLevel;
        MaxTokens = maxTokens;
        PromptTemplateId = promptTemplateId;
    }

    public static PromptConfiguration Create(
        CommitComplexityLevel complexityLevel,
        Guid promptTemplateId,
        int maxTokens = 1000)
    {
        if (maxTokens <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "MaxTokens must be greater than zero.");
        if (promptTemplateId == Guid.Empty)
            throw new ArgumentException("PromptTemplateId must be provided.", nameof(promptTemplateId));
        
        return new PromptConfiguration(
            id: Guid.NewGuid(),
            complexityLevel: complexityLevel,
            maxTokens: maxTokens,
            promptTemplateId: promptTemplateId);
    }

    public void Update(
        CommitComplexityLevel complexityLevel,
        int maxTokens,
        Guid promptTemplateId)
    {
        if (maxTokens <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "MaxTokens must be greater than zero.");
        if (promptTemplateId == Guid.Empty)
            throw new ArgumentException("PromptTemplateId must be provided.", nameof(promptTemplateId));

        ComplexityLevel = complexityLevel;
        MaxTokens = maxTokens;
        PromptTemplateId = promptTemplateId;
    }
}
