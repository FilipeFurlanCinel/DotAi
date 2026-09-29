using Domain.LlmAggregate.Models;

namespace Usecases.Llm;

public interface ILlmClient
{
    Task<string> CompleteAsync(
        LlmModel model,
        string systemPrompt,
        string userPrompt,
        int maxTokens = 1000,
        CancellationToken cancellationToken = default);
}
