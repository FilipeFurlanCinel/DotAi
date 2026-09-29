using System.Net.Http.Json;
using System.Text.Json;
using Domain.LlmAggregate;
using Domain.LlmAggregate.Enums;
using Domain.LlmAggregate.Models;
using Usecases.Llm;

namespace Infrastructure.Llm.Providers;

public sealed class GeminiLlmProvider : ILlmProvider
{
    public const string HttpClientName = nameof(GeminiLlmProvider);

    private readonly IHttpClientFactory _httpClientFactory;

    public GeminiLlmProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public LlmProviderType ProviderType => LlmProviderType.Gemini;

    public async Task<string> CompleteAsync(
        LlmModel model,
        string systemPrompt,
        string userPrompt,
        int maxTokens = 1000,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.ApiKey))
            throw new InvalidOperationException("Gemini requires an API key. Re-add the model with -k/--api-key.");

        var client = _httpClientFactory.CreateClient(HttpClientName);
        var url = BuildGenerateContentUrl(model);

        var body = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = userPrompt } }
                }
            },
            generationConfig = new
            {
                maxOutputTokens = maxTokens
            }
        };

        using var response = await client.PostAsJsonAsync(url, body, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Gemini error ({(int)response.StatusCode}): {raw}");

        using var doc = JsonDocument.Parse(raw);
        var text = ExtractText(doc.RootElement);

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Gemini returned an empty response.");

        return text.Trim();
    }

    private static string BuildGenerateContentUrl(LlmModel model)
    {
        var baseUrl = string.IsNullOrWhiteSpace(model.Endpoint)
            ? LlmProviderCatalog.Get(LlmProviderType.Gemini).DefaultEndpoint
            : model.Endpoint.Trim().TrimEnd('/');

        // Allow either base (.../v1beta) or a full generateContent URL.
        if (baseUrl.Contains(":generateContent", StringComparison.OrdinalIgnoreCase))
        {
            return AppendApiKey(baseUrl, model.ApiKey!);
        }

        var modelId = Uri.EscapeDataString(model.Name);
        var url = $"{baseUrl}/models/{modelId}:generateContent";
        return AppendApiKey(url, model.ApiKey!);
    }

    private static string AppendApiKey(string url, string apiKey)
    {
        var separator = url.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{url}{separator}key={Uri.EscapeDataString(apiKey)}";
    }

    private static string? ExtractText(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            return null;

        var content = candidates[0].GetProperty("content");
        if (!content.TryGetProperty("parts", out var parts))
            return null;

        return string.Concat(
            parts.EnumerateArray()
                .Where(p => p.TryGetProperty("text", out _))
                .Select(p => p.GetProperty("text").GetString() ?? string.Empty));
    }
}
