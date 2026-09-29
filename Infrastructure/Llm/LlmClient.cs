using Domain.LlmAggregate.Enums;
using Domain.LlmAggregate.Models;
using Usecases.Llm;

namespace Infrastructure.Llm;

/// <summary>
/// Application facade: routes completion calls to the <see cref="ILlmProvider"/> matching the model.
/// </summary>
public sealed class LlmClient : ILlmClient
{
    private readonly IReadOnlyDictionary<LlmProviderType, ILlmProvider> _providers;

    public LlmClient(IEnumerable<ILlmProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderType);
    }

    public Task<string> CompleteAsync(
        LlmModel model,
        string systemPrompt,
        string userPrompt,
        int maxTokens = 1000,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(model.ProviderType, out var provider))
        {
            throw new InvalidOperationException(
                $"No infrastructure client registered for provider '{model.ProviderType}'. " +
                "Add an ILlmProvider implementation and register it in DI.");
        }

        return provider.CompleteAsync(model, systemPrompt, userPrompt, maxTokens, cancellationToken);
    }
}
