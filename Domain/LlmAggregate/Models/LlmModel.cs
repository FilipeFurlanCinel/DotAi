using Domain._Common;
using Domain.LlmAggregate.Enums;

namespace Domain.LlmAggregate.Models;

public sealed class LlmModel : AggregateRoot<Guid>
{
    public string Name { get; }
    public LlmProviderType ProviderType { get; }
    public string? ApiKey { get; }
    public string? Endpoint { get; }
    

    private LlmModel(Guid id, string name, LlmProviderType providerType, string? apiKey, string? endpoint) : base(id)
    {
        Name = name;
        ProviderType = providerType;
        ApiKey = apiKey;
        Endpoint = endpoint;
    }

    public static LlmModel Create(string name, LlmProviderType providerType, string? apiKey = null, string? endpoint = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Model name must be provided.", nameof(name));

        return new LlmModel(
            id: Guid.NewGuid(),
            name: name.Trim(),
            providerType: providerType,
            apiKey: string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim(),
            endpoint: string.IsNullOrWhiteSpace(endpoint) ? null : endpoint.Trim());
    }

    public static LlmModel Reconstitute(Guid id, string name, LlmProviderType providerType, string? apiKey, string? endpoint)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id must not be empty.", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Model name must be provided.", nameof(name));

        return new LlmModel(
            id: id,
            name: name.Trim(),
            providerType: providerType,
            apiKey: string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim(),
            endpoint: string.IsNullOrWhiteSpace(endpoint) ? null : endpoint.Trim());
    }

    public LlmModel WithName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty.", nameof(newName));
        
        return Update(name: newName.Trim());
    }

    public LlmModel WithProviderType(LlmProviderType newProviderType)
    {
        return Update(providerType: newProviderType);
    }

    public LlmModel WithApiKey(string? newApiKey)
    {
        return Update(apiKey: string.IsNullOrWhiteSpace(newApiKey) ? null : newApiKey.Trim());
    }

    public LlmModel WithoutApiKey() => Update(apiKey: null);

    public LlmModel WithEndpoint(string? newEndpoint)
    {
        return Update(endpoint: string.IsNullOrWhiteSpace(newEndpoint) ? null : newEndpoint.Trim());
    }

    public LlmModel WithoutEndpoint() => Update(endpoint: null);

    private LlmModel Update(
        string? name = null,
        LlmProviderType? providerType = null,
        string? apiKey = null,
        string? endpoint = null)
    {
        return new LlmModel(
            id: this.Id,
            name: name ?? this.Name,
            providerType: providerType ?? this.ProviderType,
            apiKey: apiKey ?? this.ApiKey,
            endpoint: endpoint ?? this.Endpoint);
    }

    public override string ToString()
    {
        var apiKeyMasked = ApiKey is null ? "null" : "****";
        var endpointMasked = Endpoint is null ? "null" : "****";
        return $"Model {{ Name = {Name}, Provider = {ProviderType}, ApiKey = {apiKeyMasked}, Endpoint = {endpointMasked} }}";
    }
}