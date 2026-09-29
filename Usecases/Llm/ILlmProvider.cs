using Domain.LlmAggregate.Enums;
using Domain.LlmAggregate.Models;

namespace Usecases.Llm;

/// <summary>
/// Port for a single LLM vendor integration (Infrastructure implements one per <see cref="LlmProviderType"/>).
/// </summary>
public interface ILlmProvider
{
    LlmProviderType ProviderType { get; }

    Task<string> CompleteAsync(
        LlmModel model,
        string systemPrompt,
        string userPrompt,
        int maxTokens = 1000,
        CancellationToken cancellationToken = default);
}
