using Domain.LlmAggregate.Enums;

namespace Domain.LlmAggregate;

/// <summary>
/// Domain registry of supported providers (not persisted).
/// User-configured models (<see cref="Models.LlmModel"/>) reference <see cref="LlmProviderType"/>.
/// </summary>
public static class LlmProviderCatalog
{
    public sealed record ProviderInfo(
        LlmProviderType Type,
        string DisplayName,
        string DefaultEndpoint,
        string ExampleModelId,
        bool RequiresApiKey);

    public static IReadOnlyList<ProviderInfo> All { get; } =
    [
        new(
            LlmProviderType.OpenAi,
            DisplayName: "OpenAI (GPT)",
            DefaultEndpoint: "https://api.openai.com/v1/chat/completions",
            ExampleModelId: "gpt-4o-mini",
            RequiresApiKey: true),
        new(
            LlmProviderType.Gemini,
            DisplayName: "Google Gemini",
            DefaultEndpoint: "https://generativelanguage.googleapis.com/v1beta",
            ExampleModelId: "gemini-2.0-flash",
            RequiresApiKey: true)
    ];

    public static IReadOnlyList<string> Names =>
        All.Select(p => p.Type.ToString()).ToList();

    public static ProviderInfo Get(LlmProviderType type) =>
        All.FirstOrDefault(p => p.Type == type)
        ?? throw new ArgumentOutOfRangeException(nameof(type), type, "Provider is not in the catalog.");

    public static bool TryParse(string? name, out LlmProviderType provider)
    {
        provider = default;
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (Enum.TryParse(name, ignoreCase: true, out LlmProviderType parsed) &&
            All.Any(p => p.Type == parsed))
        {
            provider = parsed;
            return true;
        }

        // Friendly aliases
        if (name.Equals("GPT", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
        {
            provider = LlmProviderType.OpenAi;
            return true;
        }

        if (name.Equals("Google", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("GoogleGemini", StringComparison.OrdinalIgnoreCase))
        {
            provider = LlmProviderType.Gemini;
            return true;
        }

        return false;
    }
}
