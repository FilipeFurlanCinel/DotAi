using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.LlmAggregate;
using Domain.LlmAggregate.Enums;
using Domain.LlmAggregate.Models;
using Usecases.Llm;

namespace Infrastructure.Llm.Providers;

public sealed class OpenAiLlmProvider : ILlmProvider
{
    public const string HttpClientName = nameof(OpenAiLlmProvider);

    private readonly IHttpClientFactory _httpClientFactory;

    public OpenAiLlmProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public LlmProviderType ProviderType => LlmProviderType.OpenAi;

    public async Task<string> CompleteAsync(
        LlmModel model,
        string systemPrompt,
        string userPrompt,
        int maxTokens = 1000,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.ApiKey))
            throw new InvalidOperationException("OpenAI requires an API key. Re-add the model with -k/--api-key.");

        var client = _httpClientFactory.CreateClient(HttpClientName);
        var endpoint = ResolveEndpoint(model);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.ApiKey);

        var body = new
        {
            model = model.Name,
            max_tokens = maxTokens,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        request.Content = JsonContent.Create(body);

        using var response = await client.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"OpenAI error ({(int)response.StatusCode}): {raw}");

        using var doc = JsonDocument.Parse(raw);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("OpenAI returned an empty response.");

        return content.Trim();
    }

    private static string ResolveEndpoint(LlmModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Endpoint))
        {
            var endpoint = model.Endpoint.Trim().TrimEnd('/');
            return endpoint.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase)
                ? endpoint
                : endpoint + "/chat/completions";
        }

        return LlmProviderCatalog.Get(LlmProviderType.OpenAi).DefaultEndpoint;
    }
}
