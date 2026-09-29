using Domain._Common;

namespace Domain.ConfigurationAggregate.Models;

public sealed class PromptTemplate : Entity<Guid>
{
    public string Name { get; private set; }
    public string Content { get; private set; }
    public string Description { get; private set; }

    private PromptTemplate(Guid id, string name, string content, string description) : base(id)
    {
        Name = name;
        Content = content;
        Description = description;
    }

    public static PromptTemplate Create(string name, string content, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        return new PromptTemplate(Guid.NewGuid(), name, content, description);
    }

    public void Update(string name, string content, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        Name = name;
        Content = content;
        Description = description;
    }
}
