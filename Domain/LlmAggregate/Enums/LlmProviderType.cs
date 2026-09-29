namespace Domain.LlmAggregate.Enums;

/// <summary>
/// Closed set of LLM integrations supported by DotAi.
/// Add new values here when introducing a provider; implement matching infrastructure.
/// </summary>
public enum LlmProviderType
{
    /// <summary>OpenAI GPT models (Chat Completions API).</summary>
    OpenAi = 1,

    /// <summary>Google Gemini (Generative Language API).</summary>
    Gemini = 2
}
