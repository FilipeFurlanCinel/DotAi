using Domain._Common;
using Domain.LlmAggregate.Models;

namespace Domain.ConfigurationAggregate.Models;

public sealed class Configuration : AggregateRoot<Guid>
{
    public string Name { get; }
    public Guid LlmModelId { get; private set; }
    public LlmModel? LlmModel { get; private set; }
    public Guid PromptConfigurationId { get; private set; }
    public PromptConfiguration PromptConfig { get; private set; } = null!;
    public bool IsEnabled { get; }
    public string? CommitPrefix { get; }

    private Configuration(
        Guid id,
        string name,
        Guid llmModelId,
        Guid promptConfigurationId,
        bool isEnabled,
        string? commitPrefix) : base(id)
    {
        Name = name;
        LlmModelId = llmModelId;
        PromptConfigurationId = promptConfigurationId;
        IsEnabled = isEnabled;
        CommitPrefix = commitPrefix;
    }

    public static Configuration Create(
        string name,
        Guid llmModelId,
        PromptConfiguration promptConfig,
        bool isEnabled = true,
        string? commitPrefix = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Configuration name must be provided.", nameof(name));
        if (llmModelId == Guid.Empty)
            throw new ArgumentException("LlmModelId must be provided.", nameof(llmModelId));
        if (promptConfig is null)
            throw new ArgumentNullException(nameof(promptConfig));

        var configuration = new Configuration(
            id: Guid.NewGuid(),
            name: name.Trim(),
            llmModelId: llmModelId,
            promptConfigurationId: promptConfig.Id,
            isEnabled: isEnabled,
            commitPrefix: NormalizePrefix(commitPrefix));

        configuration.PromptConfig = promptConfig;
        return configuration;
    }

    public static Configuration Reconstitute(
        Guid id,
        string name,
        Guid llmModelId,
        Guid promptConfigurationId,
        bool isEnabled,
        string? commitPrefix = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Configuration name must be provided.", nameof(name));
        if (llmModelId == Guid.Empty)
            throw new ArgumentException("LlmModelId must be provided.", nameof(llmModelId));
        if (promptConfigurationId == Guid.Empty)
            throw new ArgumentException("PromptConfigurationId must be provided.", nameof(promptConfigurationId));

        return new Configuration(
            id,
            name.Trim(),
            llmModelId,
            promptConfigurationId,
            isEnabled,
            NormalizePrefix(commitPrefix));
    }

    public Configuration WithName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty.", nameof(newName));

        return Update(name: newName.Trim());
    }

    public Configuration WithLlm(Guid newLlmModelId)
    {
        if (newLlmModelId == Guid.Empty)
            throw new ArgumentException("Id empty", nameof(newLlmModelId));

        return Update(llmModelId: newLlmModelId);
    }

    public Configuration WithPromptConfig(PromptConfiguration newPromptConfig)
    {
        if (newPromptConfig is null)
            throw new ArgumentNullException(nameof(newPromptConfig));

        return Update(promptConfigurationId: newPromptConfig.Id, promptConfig: newPromptConfig);
    }

    public Configuration WithPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentException("Prefix cannot be empty.", nameof(prefix));

        return Update(commitPrefix: NormalizePrefix(prefix));
    }

    public Configuration ClearPrefix() => Update(commitPrefix: null, clearPrefix: true);

    public Configuration Enable() => Update(isEnabled: true);
    public Configuration Disable() => Update(isEnabled: false);

    private Configuration Update(
        string? name = null,
        Guid? llmModelId = null,
        Guid? promptConfigurationId = null,
        PromptConfiguration? promptConfig = null,
        bool? isEnabled = null,
        string? commitPrefix = null,
        bool clearPrefix = false)
    {
        var updated = new Configuration(
            id: Id,
            name: name ?? Name,
            llmModelId: llmModelId ?? LlmModelId,
            promptConfigurationId: promptConfigurationId ?? PromptConfigurationId,
            isEnabled: isEnabled ?? IsEnabled,
            commitPrefix: clearPrefix ? null : (commitPrefix ?? CommitPrefix));

        updated.PromptConfig = promptConfig ?? PromptConfig;
        updated.LlmModel = LlmModel;
        return updated;
    }

    private static string? NormalizePrefix(string? prefix) =>
        string.IsNullOrWhiteSpace(prefix) ? null : prefix.Trim();
}
